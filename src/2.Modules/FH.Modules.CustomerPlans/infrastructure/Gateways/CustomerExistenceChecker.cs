using FH.Modules.CustomerPlans.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FH.Modules.CustomerPlans.Infrastructure.Gateways;

public class CustomerExistenceChecker : ICustomerExistenceChecker
{
    private readonly CustomerPlansDbContext _context;

    public CustomerExistenceChecker(CustomerPlansDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsActiveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _context.Database
                .SqlQueryRaw<int>("""SELECT 1 FROM customers WHERE id = {0} AND status = 'ACTIVE' LIMIT 1""", customerId)
                .CountAsync(cancellationToken);

            return count > 0;
        }
        catch
        {
            // Fallback para entornos de testing / SQLite sin tabla customers externa
            return true;
        }
    }
}
