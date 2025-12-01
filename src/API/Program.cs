using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Core.Domain.Interfaces;
using Core.Application.Services;
using System.Text.Json.Serialization;
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

    // Dependency Injection - Services
    builder.Services.AddScoped<SpendingValidationService>();
    builder.Services.AddScoped<TransactionService>();
    builder.Services.AddScoped<RecurringTransactionService>();

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
