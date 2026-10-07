
using FH.Customers.Domain.Common;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Domain.Events;

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

public sealed record BeneficiaryUpdatedDomainEvent(
    Guid SubscriptionId,
    Guid MemberId,
    int DerivedAge) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "BENEFICIARY_UPDATED";
}