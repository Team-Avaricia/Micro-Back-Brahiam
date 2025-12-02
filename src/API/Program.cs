using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Core.Domain.Interfaces;
using Core.Application.Services;
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
    Log.Information("Iniciando MS Core API");

    var builder = WebApplication.CreateBuilder(args);

    // Agregar Serilog
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
        c.SwaggerDoc("v1", new() { Title = "Juez IA - MS Core API", Version = "v1" });
    });

    // Database Configuration
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Dependency Injection - Repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IFinancialRuleRepository, FinancialRuleRepository>();
    builder.Services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<ITelegramLinkCodeRepository, TelegramLinkCodeRepository>();

    // Dependency Injection - Services
    builder.Services.AddScoped<SpendingValidationService>();
    builder.Services.AddScoped<TransactionService>();
    builder.Services.AddScoped<RecurringTransactionService>();
    builder.Services.AddScoped<TokenService>();
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<TelegramService>();

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
        options.RequireHttpsMetadata = false; // En producción cambiar a true
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
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
                "http://localhost:3000",  // Dashboard (Vue.js)
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
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Juez IA - MS Core API v1"));
    }

    // Agregar Serilog request logging
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    // Enable CORS
    app.UseCors("AllowMicroservices");

    // Authentication DEBE ir antes de Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("MS Core API iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}
