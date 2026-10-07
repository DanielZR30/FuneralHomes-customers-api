using CustomerEntity = FH.Modules.Customer.Domain.Entities.Customer;
using FH.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FH.Modules.Customer.Infrastructure.Persistence;

public class CustomerSeeder
{
    private readonly CustomerModuleDbContext _context;
    private readonly ILogger<CustomerSeeder>? _logger;

    public CustomerSeeder(
        CustomerModuleDbContext context,
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

            var defaultCustomer = new CustomerEntity(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CustomerType.Individual,
                "Carlos Andrés Gómez",
                "CC",
                "1020304050",
                "carlos.gomez@example.com",
                "3001234567",
                "Carrera 45 # 26 - 85, Medellín");

            await _context.Customers.AddAsync(defaultCustomer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger?.LogInformation("CustomerSeeder: Cliente por defecto sembrado exitosamente (Id: 11111111-1111-1111-1111-111111111111).");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "CustomerSeeder: Error al sembrar clientes iniciales.");
        }
    }
}
