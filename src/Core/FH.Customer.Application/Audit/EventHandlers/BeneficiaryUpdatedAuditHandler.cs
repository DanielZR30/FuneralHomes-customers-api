using FH.Customer.Application.Events;
using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using MediatR;

namespace FH.Modules.Audit.Application.EventHandlers;

public class BeneficiaryUpdatedAuditHandler : INotificationHandler<BeneficiaryUpdatedAppEvent>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public BeneficiaryUpdatedAuditHandler(IBeneficiaryAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public Task Handle(BeneficiaryUpdatedAppEvent notification, CancellationToken cancellationToken)
    {
        var log = BeneficiaryAuditLog.Create(
            notification.SubscriptionId,
            notification.MemberId,
            AuditAction.BeneficiaryUpdated);

        return _auditLogs.AddAsync(log, cancellationToken);
    }
}
