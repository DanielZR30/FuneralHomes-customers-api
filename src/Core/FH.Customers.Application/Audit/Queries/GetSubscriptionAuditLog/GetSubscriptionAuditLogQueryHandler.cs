
using FH.Customers.Application.Audit.DTOs;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Audit.Repositories;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Application.Audit.Queries.GetSubscriptionAuditLog;

public class GetSubscriptionAuditLogQueryHandler
    : IQueryHandler<GetSubscriptionAuditLogQuery, IReadOnlyList<AuditLogResponse>>
{
    private readonly IRepository<Subscription, Guid> _subscriptions;
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public GetSubscriptionAuditLogQueryHandler(
        IRepository<Subscription, Guid> subscriptions,
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
