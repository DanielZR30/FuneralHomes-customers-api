using FH.Customers.Domain.Common;

namespace FH.Customers.Domain.Common.Events;

public sealed record SubscriptionCreated(
    Guid SubscriptionId,
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly StartDate,
    DateOnly? EndDate) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(SubscriptionCreated);
}
