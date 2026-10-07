using FH.Customer.Api.Extensions;
using FH.Modules.Audit.Infrastructure.Endpoints;
using FH.Modules.Beneficiaries.Infrastructure.Endpoints;
using FH.Modules.Customer.Infrastructure.Endpoints;
using FH.Modules.CustomerPlans.Infrastructure.Endpoints;
using FH.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Base de Datos PostgreSQL con Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("CustomerDb")
    ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// 2. Registro de todas las capas de la arquitectura (Domain, Application, Infrastructure)
builder.Services.AddApplicationServices();

// Configuración de serialización JSON con Enums legibles (ej: "Individual", "Human", "Child")
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Configuración de Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Funeral Homes - Customers Microservice API",
        Version = "v1",
        Description = "Microservicio para la gestión de titulares (B2C/B2B), suscripciones a planes y beneficiarios (fh.api.customer) con Clean Architecture."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Funeral Homes Customers API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Manejo centralizado de excepciones con ProblemDetails (RN-09, BusinessRule -> 422, Validation -> 400, NotFound -> 404)
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var ex = exceptionHandlerPathFeature?.Error;

        var statusCode = ex switch
        {
            FH.Shared.Domain.Exceptions.BusinessRuleException => StatusCodes.Status422UnprocessableEntity,
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = ex switch
            {
                FH.Shared.Domain.Exceptions.BusinessRuleException => "Regla de Negocio Incumplida",
                FluentValidation.ValidationException => "Error de Validación",
                KeyNotFoundException => "Recurso No Encontrado",
                _ => "Error Interno del Servidor"
            },
            Detail = ex?.Message
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Redireccionar raíz a swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// Endpoint de salud del microservicio
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "fh.api.customer",
    timestamp = DateTime.UtcNow
}))
.WithTags("Health")
.WithSummary("Verifica el estado de salud del microservicio");

// Mapeo de Endpoints de cada módulo
app.MapCustomerEndpoints();
app.MapSubscriptionEndpoints();
app.MapBeneficiaryEndpoints();
app.MapAuditEndpoints();

// Inicialización de la base de datos y seeder al arrancar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetService<ILogger<Program>>();
    try
    {
        var db = services.GetService<CustomerDbContext>();
        if (db is not null)
        {
            var databaseCreator = db.Database.GetService<IRelationalDatabaseCreator>();
            try
            {
                await databaseCreator.CreateTablesAsync();
            }
            catch
            {
                // Tablas ya creadas
            }
        }

        var seeder = services.GetService<FH.Modules.CustomerPlans.Infrastructure.IDataSeeder>();
        if (seeder is not null)
        {
            await seeder.SeedAsync();
        }

        logger?.LogInformation("Inicialización de Base de Datos y Seeders completada.");
    }
    catch (Exception ex)
    {
        logger?.LogWarning(ex, "No se pudo conectar a la base de datos para la inicialización.");
    }
}

app.Run();
