using FH.Modules.CustomerPlans.Domain;
using FH.Modules.CustomerPlans.Domain.ValueObjects;
using FH.Modules.CustomerPlans.Infrastructure;
using FH.Shared.Domain.Enums;
using FH.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FH.Customer.Api.Infrastructure;

public class SubscriptionSeeder : IDataSeeder
{
    private readonly CustomerDbContext _context;
    private readonly ILogger<SubscriptionSeeder>? _logger;

    // Orden de ejecución posterior al seeder de clientes (clientes = 10, suscripciones = 20)
    public int Order => 20;

    public SubscriptionSeeder(
        CustomerDbContext context,
        ILogger<SubscriptionSeeder>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _context.Subscriptions.AnyAsync(cancellationToken))
            {
                return;
            }

            var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var externalPlanId = Guid.Parse("99999999-9999-9999-9999-999999999999");

            var subscription = Subscription.Create(
                customerId,
                externalPlanId,
                new MaxBeneficiaries(4),
                new DateOnly(2026, 1, 15));

            await _context.Subscriptions.AddAsync(subscription, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger?.LogInformation("SubscriptionSeeder: Datos iniciales de suscripciones sembrados exitosamente.");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "SubscriptionSeeder: No se pudo sembrar suscripciones iniciales en BD.");
        }
    }
}
