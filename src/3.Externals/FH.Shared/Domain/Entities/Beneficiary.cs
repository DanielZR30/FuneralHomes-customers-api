using FH.Shared.Domain.Common;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Exceptions;

namespace FH.Shared.Domain.Entities;

public class Beneficiary : AggregateRoot<Guid>
{
    public Guid SubscriptionId { get; private set; }
    public Guid MemberId { get; private set; }
    public BeneficiaryType BeneficiaryType { get; private set; }
    public RelationshipType RelationshipType { get; private set; }
    public BeneficiaryStatus Status { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? RemovedAt { get; private set; }

    // Navigation properties
    public CustomerSubscription? Subscription { get; private set; }
    public Member? Member { get; private set; }

    // EF Core constructor
    private Beneficiary() : base() { }

    public Beneficiary(
        Guid id,
        Guid subscriptionId,
        Guid memberId,
        BeneficiaryType beneficiaryType,
        RelationshipType relationshipType) : base(id)
    {
        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("El ID de la suscripción es obligatorio.", nameof(subscriptionId));

        if (memberId == Guid.Empty)
            throw new ArgumentException("El ID del miembro es obligatorio.", nameof(memberId));

        SubscriptionId = subscriptionId;
        MemberId = memberId;
        BeneficiaryType = beneficiaryType;
        RelationshipType = relationshipType;
        Status = BeneficiaryStatus.Active;
        JoinedAt = DateTime.UtcNow;
    }

    public static Beneficiary Create(
        Guid subscriptionId,
        Guid memberId,
        BeneficiaryType beneficiaryType,
        RelationshipType relationshipType)
    {
        return new Beneficiary(
            Guid.NewGuid(),
            subscriptionId,
            memberId,
            beneficiaryType,
            relationshipType);
    }

    public static Beneficiary Enroll(
    Guid subscriptionId,
    Member member,
    BeneficiaryType beneficiaryType,
    RelationshipType relationshipType)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (member.SubjectType == SubjectType.Pet && relationshipType != RelationshipType.Pet)
            throw new BusinessRuleException("Un sujeto de tipo Mascota (Pet) debe tener relación 'Pet'.");

        return Create(subscriptionId, member.Id, beneficiaryType, relationshipType);
    }

    public void Remove()
    {
        if (Status == BeneficiaryStatus.Removed)
            throw new BusinessRuleException("El beneficiario ya fue retirado de esta suscripción.");

        Status = BeneficiaryStatus.Removed;
        RemovedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Status = BeneficiaryStatus.Active;
        RemovedAt = null;
    }
}
