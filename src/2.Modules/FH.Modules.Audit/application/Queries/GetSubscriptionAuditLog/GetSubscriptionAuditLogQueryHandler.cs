using FH.Modules.Audit.Application.DTOs;
using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Repositories;

namespace FH.Modules.Audit.Application.Queries.GetSubscriptionAuditLog;

public class GetSubscriptionAuditLogQueryHandler
    : IQueryHandler<GetSubscriptionAuditLogQuery, IReadOnlyList<AuditLogResponse>>
{
    private readonly IRepository<CustomerSubscription, Guid> _subscriptions;
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public GetSubscriptionAuditLogQueryHandler(
        IRepository<CustomerSubscription, Guid> subscriptions,
        IBeneficiaryAuditLogRepository auditLogs)
    {
        _subscriptions = subscriptions;
        _auditLogs = auditLogs;
    }

    public async Task<Result<IReadOnlyList<AuditLogResponse>>> Handle(
        GetSubscriptionAuditLogQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptions.GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result<IReadOnlyList<AuditLogResponse>>.NotFound(
                $"La suscripción {request.SubscriptionId} no existe.");
        }

        var logs = await _auditLogs.GetBySubscriptionAsync(request.SubscriptionId, cancellationToken);

        IReadOnlyList<AuditLogResponse> response = logs
            .Select(AuditLogResponse.FromEntity)
            .ToList();

        return Result<IReadOnlyList<AuditLogResponse>>.Success(response);
    }
}
