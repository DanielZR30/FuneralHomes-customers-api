using FH.Shared.Domain.Common;

namespace FH.Modules.CustomerPlans.Domain.Events;

public sealed record SubscriptionCancelled(
    Guid SubscriptionId,
    Guid CustomerId,
    DateOnly CancelledDate) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => nameof(SubscriptionCancelled);
}
