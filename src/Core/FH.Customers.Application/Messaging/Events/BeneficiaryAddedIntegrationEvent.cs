namespace FH.Customers.Application.Messaging.Events;

public record BeneficiaryAddedData(
    Guid SubscriptionId,
    Guid MemberId,
    string SubjectType,
    int DerivedAge,
    string RelationshipType,
    string BeneficiaryType);

public record BeneficiaryAddedIntegrationEvent(
    Guid EventId,
    string EventType,
    DateTime Timestamp,
    BeneficiaryAddedData Data);
