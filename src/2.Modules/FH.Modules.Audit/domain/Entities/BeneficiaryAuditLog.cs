using FH.Shared.Domain.Common;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Audit.Domain.Entities;

public class BeneficiaryAuditLog : BaseEntity<Guid>
{
    public Guid SubscriptionId { get; private set; }
    public Guid MemberId { get; private set; }
    public AuditAction Action { get; private set; }
    public bool EventPublished { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private BeneficiaryAuditLog() : base() { }

    public BeneficiaryAuditLog(
        Guid id,
        Guid subscriptionId,
        Guid memberId,
        AuditAction action,
        bool eventPublished = false) : base(id)
    {
        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("El ID de suscripción es obligatorio.", nameof(subscriptionId));

        if (memberId == Guid.Empty)
            throw new ArgumentException("El ID de miembro es obligatorio.", nameof(memberId));

        SubscriptionId = subscriptionId;
        MemberId = memberId;
        Action = action;
        EventPublished = eventPublished;
        CreatedAt = DateTime.UtcNow;
    }

    public static BeneficiaryAuditLog Create(
        Guid subscriptionId,
        Guid memberId,
        AuditAction action,
        bool eventPublished = false)
    {
        return new BeneficiaryAuditLog(
            Guid.NewGuid(),
            subscriptionId,
            memberId,
            action,
            eventPublished);
    }

    public void MarkEventPublished() => EventPublished = true;
}
