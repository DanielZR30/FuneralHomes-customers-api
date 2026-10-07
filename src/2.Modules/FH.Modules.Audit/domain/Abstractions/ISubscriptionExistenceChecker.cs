namespace FH.Modules.Audit.Domain.Abstractions;

public interface ISubscriptionExistenceChecker
{
    Task<bool> ExistsAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
