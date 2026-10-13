using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.Audit.DTOs;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Audit.Queries.GetUnpublishedAuditLogs;

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
