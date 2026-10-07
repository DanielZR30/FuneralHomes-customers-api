using FH.Modules.Audit.Extensions;
using FH.Modules.Beneficiaries.Extensions;
using FH.Modules.Customer.Extensions;
using FH.Modules.CustomerPlans.Extensions;
using FH.Modules.Messaging.Extensions;
using FH.Shared.Extensions;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRO DE MÓDULOS DE LA APLICACIÓN
builder.Services.AddCustomerModule(builder.Configuration);
builder.Services.AddCustomerPlansModule(builder.Configuration);
builder.Services.AddBeneficiariesModule(builder.Configuration);
builder.Services.AddAuditModule(builder.Configuration);
builder.Services.AddMessagingModule(builder.Configuration);

// Infraestructura compartida: MediatR (CQRS)
builder.Services.AddSharedCqrs(
    typeof(BeneficiariesModuleExtensions).Assembly,
    typeof(CustomerModuleExtensions).Assembly,
    typeof(CustomerPlansModuleExtensions).Assembly,
    typeof(AuditModuleExtensions).Assembly);

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
        Description = "Microservicio para la gestión de titulares (B2C/B2B), suscripciones a planes y beneficiarios (fh.api.customer)."
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

// 2. MAPEO MODULAR DE RUTAS DE CADA MÓDULO
app.MapCustomerModuleEndpoints();
app.MapCustomerPlansModuleEndpoints();
app.MapBeneficiariesModuleEndpoints();
app.MapAuditModuleEndpoints();

// Inicialización y Seeding de Base de Datos al arranque
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetService<ILogger<Program>>();

    try
    {
        // 1. Asegurar esquema en BD para cada módulo
        var customerDb = services.GetService<FH.Modules.Customer.Infrastructure.Persistence.CustomerModuleDbContext>();
        if (customerDb is not null) await customerDb.Database.EnsureCreatedAsync();

        var plansDb = services.GetService<FH.Modules.CustomerPlans.Infrastructure.CustomerPlansDbContext>();
        if (plansDb is not null) await plansDb.Database.EnsureCreatedAsync();

        var beneficiariesDb = services.GetService<FH.Modules.Beneficiaries.Infrastructure.Persistence.BeneficiariesDbContext>();
        if (beneficiariesDb is not null) await beneficiariesDb.Database.EnsureCreatedAsync();

        var auditDb = services.GetService<FH.Modules.Audit.Infrastructure.Persistence.AuditDbContext>();
        if (auditDb is not null) await auditDb.Database.EnsureCreatedAsync();

        // 2. Sembrar datos de prueba
        var customerSeeder = services.GetService<FH.Modules.Customer.Infrastructure.Persistence.CustomerSeeder>();
        if (customerSeeder is not null) await customerSeeder.SeedAsync();

        var plansSeeder = services.GetService<FH.Modules.CustomerPlans.Infrastructure.IDataSeeder>();
        if (plansSeeder is not null) await plansSeeder.SeedAsync();

        logger?.LogInformation("Inicialización y sembrado de datos completado exitosamente.");
    }
    catch (Exception ex)
    {
        logger?.LogWarning(ex, "No se pudo conectar a la base de datos para EnsureCreated/Seeders. Asegúrate de levantar PostgreSQL.");
    }
}

app.Run();


