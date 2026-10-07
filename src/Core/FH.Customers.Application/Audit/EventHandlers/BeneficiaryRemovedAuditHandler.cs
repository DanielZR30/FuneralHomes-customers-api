
using FH.Customers.Application.Events;
using FH.Customers.Domain.Audit.Repositories;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Events;

namespace FH.Customers.Application.Audit.EventHandlers;

public class BeneficiaryRemovedAuditHandler : IDomainEventHandler<BeneficiaryRemovedDomainEvent>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public BeneficiaryRemovedAuditHandler(IBeneficiaryAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public Task Handle(BeneficiaryRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        var log = BeneficiaryAuditLog.Create(
            notification.SubscriptionId,
            notification.MemberId,
            AuditAction.BeneficiaryRemoved);

        return _auditLogs.AddAsync(log, cancellationToken);
    }
}
