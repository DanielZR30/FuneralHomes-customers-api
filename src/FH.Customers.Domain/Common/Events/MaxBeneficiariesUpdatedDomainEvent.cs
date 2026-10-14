using FH.Customers.Domain.Common;

namespace FH.Customers.Domain.Common.Events;

public sealed record MaxBeneficiariesUpdatedDomainEvent(
    Guid SubscriptionId,
    int PreviousMax,
    int NewMax) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "MAX_BENEFICIARIES_UPDATED";
}
