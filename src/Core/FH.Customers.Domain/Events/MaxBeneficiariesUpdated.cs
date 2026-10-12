using FH.Customers.Domain.Common;

namespace FH.Customers.Domain.Events;

public sealed record MaxBeneficiariesUpdated(
    Guid SubscriptionId,
    int PreviousMax,
    int NewMax) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(MaxBeneficiariesUpdated);
}
