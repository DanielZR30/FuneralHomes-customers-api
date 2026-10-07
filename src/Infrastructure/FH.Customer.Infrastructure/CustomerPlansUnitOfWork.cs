using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Shared.Persistence;

namespace FH.Customer.Api.Infrastructure;

public class CustomerPlansUnitOfWork : ICustomerPlansUnitOfWork
{
    private readonly CustomerDbContext _context;

    public CustomerPlansUnitOfWork(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
