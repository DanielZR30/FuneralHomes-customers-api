using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.Audit.DTOs;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Audit.Queries.GetMemberAuditLog;

public class GetMemberAuditLogQueryHandler
    : IQueryHandler<GetMemberAuditLogQuery, IReadOnlyList<AuditLogResponse>>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;

    public GetMemberAuditLogQueryHandler(IBeneficiaryAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<Result<IReadOnlyList<AuditLogResponse>>> Handle(
        GetMemberAuditLogQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await _auditLogs.GetByMemberAsync(request.MemberId, cancellationToken);

        IReadOnlyList<AuditLogResponse> response = logs
            .Select(AuditLogResponse.FromEntity)
            .ToList();

        return Result<IReadOnlyList<AuditLogResponse>>.Success(response);
    }
}
