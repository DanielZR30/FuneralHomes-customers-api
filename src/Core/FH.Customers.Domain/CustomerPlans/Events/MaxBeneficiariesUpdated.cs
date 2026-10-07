
using FH.Customers.Domain.Common;
using FH.Customers.Domain.CustomerPlans;

namespace FH.Customers.Domain.CustomerPlans.Events;

public sealed record MaxBeneficiariesUpdated(
    Guid SubscriptionId,
    int PreviousMax,
    int NewMax) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(MaxBeneficiariesUpdated);
}
