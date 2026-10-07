using FH.Modules.Audit.Application.DTOs;
using FH.Modules.Audit.Domain.Abstractions;
using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Audit.Application.Queries.GetSubscriptionAuditLog;

public class GetSubscriptionAuditLogQueryHandler
    : IQueryHandler<GetSubscriptionAuditLogQuery, IReadOnlyList<AuditLogResponse>>
{
    private readonly ISubscriptionExistenceChecker _subscriptionChecker;
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public GetSubscriptionAuditLogQueryHandler(
        ISubscriptionExistenceChecker subscriptionChecker,
        IBeneficiaryAuditLogRepository auditLogs)
    {
        _subscriptionChecker = subscriptionChecker;
        _auditLogs = auditLogs;
    }

    public async Task<Result<IReadOnlyList<AuditLogResponse>>> Handle(
        GetSubscriptionAuditLogQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await _subscriptionChecker.ExistsAsync(request.SubscriptionId, cancellationToken);

        if (!exists)
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
