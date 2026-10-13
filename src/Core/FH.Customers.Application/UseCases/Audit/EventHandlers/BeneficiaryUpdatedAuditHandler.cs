using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Common.Events;
using FH.Customers.Domain.Common.Enums;
using FH.Customers.Application.Utilities.Events;

namespace FH.Customers.Application.UseCases.Audit.EventHandlers;

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
