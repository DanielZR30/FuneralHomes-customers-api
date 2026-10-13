using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FH.Customers.Domain.Common.Enums;
using FH.Customers.Domain.Entities;

namespace FH.Customers.Persistence.Seeds;

public class CustomerSeeder : IDataSeeder
{
    private readonly CustomerDbContext _context;
    private readonly ILogger<CustomerSeeder>? _logger;

    // Se ejecuta antes que el seeder de suscripciones (clientes = 10, suscripciones = 20),
    // porque cada suscripción referencia a un cliente existente.
    public int Order => 10;

    public CustomerSeeder(
        CustomerDbContext context,
        ILogger<CustomerSeeder>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _context.Customers.AnyAsync(cancellationToken))
            {
                return;
            }

            var customer = new Customer(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CustomerType.Individual,
                "Cliente Demo",
                "CC",
                "1000000001",
                "demo@example.com",
                "+573000000000",
                "Calle 1 # 2-3");

            await _context.Customers.AddAsync(customer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger?.LogInformation("CustomerSeeder: Cliente inicial sembrado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "CustomerSeeder: No se pudo sembrar el cliente inicial en BD.");
        }
    }
}
