using FH.Modules.Audit.Domain.Abstractions;
using FH.Modules.Audit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FH.Modules.Audit.Infrastructure.Gateways;

public class SqlSubscriptionExistenceChecker : ISubscriptionExistenceChecker
{
    private readonly AuditDbContext _context;

    public SqlSubscriptionExistenceChecker(AuditDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.Database
                .SqlQueryRaw<int>("""SELECT 1 FROM customer_subscriptions WHERE id = {0} LIMIT 1""", subscriptionId)
                .CountAsync(cancellationToken);

            return count > 0;
        }
        catch
        {
            return true;
        }
    }
}
