namespace FH.Modules.Customer.Application.Abstractions;

public interface ICustomerUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
