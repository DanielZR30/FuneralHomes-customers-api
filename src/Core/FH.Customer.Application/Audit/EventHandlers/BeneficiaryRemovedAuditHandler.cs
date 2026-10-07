using FH.Customer.Application.Events;
using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using MediatR;

namespace FH.Modules.Audit.Application.EventHandlers;

public class BeneficiaryRemovedAuditHandler : INotificationHandler<BeneficiaryRemovedAppEvent>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public BeneficiaryRemovedAuditHandler(IBeneficiaryAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public Task Handle(BeneficiaryRemovedAppEvent notification, CancellationToken cancellationToken)
    {
        var log = BeneficiaryAuditLog.Create(
            notification.SubscriptionId,
            notification.MemberId,
            AuditAction.BeneficiaryRemoved);

        return _auditLogs.AddAsync(log, cancellationToken);
    }
}
