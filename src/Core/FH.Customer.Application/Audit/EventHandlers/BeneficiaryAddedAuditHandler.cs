using FH.Customer.Application.Events;
using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using MediatR;

namespace FH.Modules.Audit.Application.EventHandlers;

public class BeneficiaryAddedAuditHandler : INotificationHandler<BeneficiaryAddedAppEvent>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public BeneficiaryAddedAuditHandler(IBeneficiaryAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public Task Handle(BeneficiaryAddedAppEvent notification, CancellationToken cancellationToken)
    {
        var log = BeneficiaryAuditLog.Create(
            notification.SubscriptionId,
            notification.MemberId,
            AuditAction.BeneficiaryAdded);

        return _auditLogs.AddAsync(log, cancellationToken);
    }
}
