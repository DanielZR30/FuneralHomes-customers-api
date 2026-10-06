using FH.Modules.CustomerPlans.Domain;

namespace FH.Modules.CustomerPlans.Application.Abstractions;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subscription>> GetActiveByCustomerAndPlanAsync(Guid customerId, Guid externalPlanId, CancellationToken cancellationToken = default);

    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);

    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
