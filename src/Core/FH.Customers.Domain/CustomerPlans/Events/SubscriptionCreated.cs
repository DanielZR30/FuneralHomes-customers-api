
using FH.Customers.Domain.Common;
using FH.Customers.Domain.CustomerPlans;

namespace FH.Customers.Domain.CustomerPlans.Events;

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
