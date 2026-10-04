using FH.Shared.Domain.Common;
using FH.Shared.Domain.Enums;

namespace FH.Shared.Domain.Events;

public sealed record BeneficiaryAddedDomainEvent(
    Guid SubscriptionId,
    Guid MemberId,
    SubjectType SubjectType,
    int DerivedAge,
    RelationshipType RelationshipType,
    BeneficiaryType BeneficiaryType) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "BENEFICIARY_ADDED";
}

public sealed record BeneficiaryRemovedDomainEvent(
    Guid SubscriptionId,
    Guid MemberId,
    RelationshipType RelationshipType,
    BeneficiaryType BeneficiaryType) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "BENEFICIARY_REMOVED";
}