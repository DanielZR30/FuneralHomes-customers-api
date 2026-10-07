
using FH.Customers.Domain.Entities;

namespace FH.Customers.Application.Audit.DTOs;

public record AuditLogResponse(
    Guid Id,
    Guid SubscriptionId,
    Guid MemberId,
    string Action,
    bool EventPublished,
    DateTime CreatedAt)
{
    public static AuditLogResponse FromEntity(BeneficiaryAuditLog log) =>
        new(
            log.Id,
            log.SubscriptionId,
            log.MemberId,
            log.Action.ToString(),
            log.EventPublished,
            log.CreatedAt);
}
