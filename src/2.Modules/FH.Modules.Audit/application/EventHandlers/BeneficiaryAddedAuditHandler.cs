using FH.Modules.Audit.Domain.Repositories;
using FH.Modules.Audit.Domain.Entities;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Events;
using MediatR;

namespace FH.Modules.Audit.Application.EventHandlers;

public class BeneficiaryAddedAuditHandler : INotificationHandler<BeneficiaryAddedDomainEvent>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public BeneficiaryAddedAuditHandler(IBeneficiaryAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public Task Handle(BeneficiaryAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var log = BeneficiaryAuditLog.Create(
            notification.SubscriptionId,
            notification.MemberId,
            AuditAction.BeneficiaryAdded);

        return _auditLogs.AddAsync(log, cancellationToken);
    }
}
