using FH.Api.Customer.Endpoints;
using FH.Api.Customer.MockData;
using FH.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Base de Datos EF Core (fh.db.customer con PostgreSQL para migraciones)
var connectionString = builder.Configuration.GetConnectionString("CustomerDb")
    ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("FH.Shared");
    });
});

// Almacén Mock en memoria (permite probar la API y Swagger sin requerir PostgreSQL levantado)
builder.Services.AddSingleton<InMemoryCustomerStore>();

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

// Mapeo modular de rutas de la API
app.MapCustomerEndpoints();
app.MapSubscriptionEndpoints();
app.MapBeneficiaryEndpoints();
app.MapAuditEndpoints();

app.Run();


