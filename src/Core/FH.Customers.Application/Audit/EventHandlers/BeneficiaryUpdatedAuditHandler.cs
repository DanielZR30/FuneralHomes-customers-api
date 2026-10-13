
using FH.Customers.Application.Events;
using FH.Customers.Domain.Repositories;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Common.Events;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.Audit.EventHandlers;

public class BeneficiaryUpdatedAuditHandler : IDomainEventHandler<BeneficiaryUpdatedDomainEvent>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public BeneficiaryUpdatedAuditHandler(IBeneficiaryAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public Task Handle(BeneficiaryUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var log = BeneficiaryAuditLog.Create(
            notification.SubscriptionId,
            notification.MemberId,
            AuditAction.BeneficiaryUpdated);

        return _auditLogs.AddAsync(log, cancellationToken);
    }
}
