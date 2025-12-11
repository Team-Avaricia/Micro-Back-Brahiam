using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Core.Domain.Interfaces;
using Core.Application.Services;
using Core.Application.Interfaces;
using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// Configurar Serilog ANTES de crear el builder
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/ms-core-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting MS Core API");

    var builder = WebApplication.CreateBuilder(args);

    // Add environment variables configuration
    builder.Configuration.AddEnvironmentVariables();

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Configurar para que los Enums se serialicen como strings en lugar de números
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Riwi Wallet - MS Core API", Version = "v1" });
    });

    // Database Configuration
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Dependency Injection - Repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IFinancialRuleRepository, FinancialRuleRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<ITelegramLinkCodeRepository, TelegramLinkCodeRepository>();

    // Dependency Injection - Services
    builder.Services.AddScoped<ISpendingValidationService, SpendingValidationService>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITelegramService, TelegramService>();

    // JWT Authentication Configuration
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };

        // Logging detallado
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error($"❌ JWT Authentication failed: {context.Exception.Message}");
                Log.Error($"Exception: {context.Exception.GetType().Name}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("✅ JWT Token validated successfully");
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}");
                if (claims != null)
                {
                    Log.Information($"Claims: {string.Join(", ", claims)}");
                }
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    Log.Information($"📨 Received Authorization header");
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // CORS Configuration for Microservices
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowMicroservices", policy =>
        {
            policy.WithOrigins(
                "http://localhost:8080",  // MS AI Worker (Spring Boot)
                "http://localhost:5173",  // Dashboard (Vue.js)
                "http://localhost:8081"   // Gateway (Spring Boot)
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
    }

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Juez IA - MS Core API v1"));

    // Agregar Serilog request logging
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    // Enable CORS
    app.UseCors("AllowMicroservices");
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("MS Core API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
