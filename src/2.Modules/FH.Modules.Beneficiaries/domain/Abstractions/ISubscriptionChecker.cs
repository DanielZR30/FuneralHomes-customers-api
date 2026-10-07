namespace FH.Modules.Beneficiaries.Domain.Abstractions;

public record SubscriptionSnapshot(Guid Id, bool IsActive, int MaxBeneficiaries)
{
    public bool CanAcceptBeneficiary(int currentCount) => IsActive && currentCount < MaxBeneficiaries;
}

public interface ISubscriptionChecker
{
    Task<SubscriptionSnapshot?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
