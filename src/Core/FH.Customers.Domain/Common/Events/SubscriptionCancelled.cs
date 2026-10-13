using FH.Customers.Domain.Common;

namespace FH.Customers.Domain.Common.Events;

public sealed record SubscriptionCancelled(
    Guid SubscriptionId,
    Guid CustomerId,
    DateOnly CancelledDate) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(SubscriptionCancelled);
}
