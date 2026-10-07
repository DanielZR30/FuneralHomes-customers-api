
using FH.Customers.Application.Audit.DTOs;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Audit.Repositories;

namespace FH.Customers.Application.Audit.Queries.GetMemberAuditLog;

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
