using FH.Shared.Domain.Common;
using FH.Shared.Domain.Enums;
using MediatR;

namespace FH.Customer.Application.Events;

public sealed record BeneficiaryAddedAppEvent(
    Guid SubscriptionId,
    Guid MemberId,
    SubjectType SubjectType,
    int DerivedAge,
    RelationshipType RelationshipType,
    BeneficiaryType BeneficiaryType) : INotification, IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "BENEFICIARY_ADDED";
}

public sealed record BeneficiaryRemovedAppEvent(
    Guid SubscriptionId,
    Guid MemberId,
    RelationshipType RelationshipType,
    BeneficiaryType BeneficiaryType) : INotification, IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "BENEFICIARY_REMOVED";
}

public sealed record BeneficiaryUpdatedAppEvent(
    Guid SubscriptionId,
    Guid MemberId,
    int DerivedAge) : INotification, IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "BENEFICIARY_UPDATED";
}
