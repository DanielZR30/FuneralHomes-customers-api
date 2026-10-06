using FH.Modules.Audit.Application.DTOs;
using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Audit.Application.Queries.GetMemberAuditLog;

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
