using FH.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Base de Datos EF Core (fh.db.customer con PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("CustomerDb")
    ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

builder.Services.AddDbContext<CustomerDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly("FH.Shared");
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

// Endpoint de salud del microservicio
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "fh.api.customer",
    timestamp = DateTime.UtcNow
}))
.WithTags("Health");

app.Run();
