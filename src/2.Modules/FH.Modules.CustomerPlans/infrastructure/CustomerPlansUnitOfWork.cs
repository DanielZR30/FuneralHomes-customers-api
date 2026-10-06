using FH.Modules.CustomerPlans.Application.Abstractions;

namespace FH.Modules.CustomerPlans.Infrastructure;

public class CustomerPlansUnitOfWork : ICustomerPlansUnitOfWork
{
    private readonly CustomerPlansDbContext _context;

    public CustomerPlansUnitOfWork(CustomerPlansDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
