namespace FH.Modules.CustomerPlans.Application.Abstractions;

public interface ICustomerPlansUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
