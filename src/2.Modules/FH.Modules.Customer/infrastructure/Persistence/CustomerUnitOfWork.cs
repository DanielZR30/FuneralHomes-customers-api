using FH.Modules.Customer.Application.Abstractions;
using FH.Modules.Customer.Infrastructure.Persistence;

namespace FH.Modules.Customer.Infrastructure.Persistence;

public class CustomerUnitOfWork : ICustomerUnitOfWork
{
    private readonly CustomerModuleDbContext _context;

    public CustomerUnitOfWork(CustomerModuleDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
