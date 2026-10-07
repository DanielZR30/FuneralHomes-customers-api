
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Domain.CustomerPlans;

namespace FH.Customers.Application.CustomerPlans.Abstractions;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subscription>> GetActiveByCustomerAndPlanAsync(Guid customerId, Guid externalPlanId, CancellationToken cancellationToken = default);

    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
