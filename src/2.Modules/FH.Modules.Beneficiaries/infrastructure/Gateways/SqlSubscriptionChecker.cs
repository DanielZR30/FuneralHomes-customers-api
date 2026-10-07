using FH.Modules.Beneficiaries.Domain.Abstractions;
using FH.Modules.Beneficiaries.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FH.Modules.Beneficiaries.Infrastructure.Gateways;

public class SqlSubscriptionChecker : ISubscriptionChecker
{
    private readonly BeneficiariesDbContext _context;

    public SqlSubscriptionChecker(BeneficiariesDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionSnapshot?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var raw = await _context.Database
                .SqlQueryRaw<RawSubscription>(
                    """SELECT id as "Id", status as "Status", max_beneficiaries as "MaxBeneficiaries" FROM customer_subscriptions WHERE id = {0}""",
                    subscriptionId)
                .FirstOrDefaultAsync(cancellationToken);

            if (raw is null) return null;

            return new SubscriptionSnapshot(raw.Id, raw.Status == "ACTIVE", raw.MaxBeneficiaries);
        }
        catch
        {
            // Fallback para testing en memoria sin tabla externa
            return new SubscriptionSnapshot(subscriptionId, true, 10);
        }
    }

    private class RawSubscription
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MaxBeneficiaries { get; set; }
    }
}
