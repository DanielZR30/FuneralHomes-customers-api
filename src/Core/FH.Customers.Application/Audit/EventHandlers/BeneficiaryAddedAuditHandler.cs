
using FH.Customers.Application.Events;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Common.Events;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.Audit.EventHandlers;

public class BeneficiaryAddedAuditHandler : IDomainEventHandler<BeneficiaryAddedDomainEvent>
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
