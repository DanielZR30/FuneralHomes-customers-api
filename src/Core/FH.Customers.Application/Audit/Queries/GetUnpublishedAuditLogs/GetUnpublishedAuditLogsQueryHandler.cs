
using FH.Customers.Application.Audit.DTOs;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Audit.Repositories;

namespace FH.Customers.Application.Audit.Queries.GetUnpublishedAuditLogs;

public class GetUnpublishedAuditLogsQueryHandler
    : IQueryHandler<GetUnpublishedAuditLogsQuery, IReadOnlyList<AuditLogResponse>>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public GetUnpublishedAuditLogsQueryHandler(IBeneficiaryAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<Result<IReadOnlyList<AuditLogResponse>>> Handle(
        GetUnpublishedAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.BatchSize <= 0)
        {
            return Result<IReadOnlyList<AuditLogResponse>>.Failure(
                "El tamaño del lote debe ser mayor que cero.");
        }

        var logs = await _auditLogs.GetUnpublishedAsync(request.BatchSize, cancellationToken);

        IReadOnlyList<AuditLogResponse> response = logs
            .Select(AuditLogResponse.FromEntity)
            .ToList();

        return Result<IReadOnlyList<AuditLogResponse>>.Success(response);
    }
}
