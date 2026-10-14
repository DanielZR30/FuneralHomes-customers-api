using FH.Customers.Domain.Common;

namespace FH.Customers.Domain.Common.Events;

public sealed record SubscriptionCancelledDomainEvent(
    Guid SubscriptionId,
    Guid CustomerId,
    DateOnly CancelledDate) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "SUBSCRIPTION_CANCELLED";
}
