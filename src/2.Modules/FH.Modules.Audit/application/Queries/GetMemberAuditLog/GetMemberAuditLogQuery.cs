using FH.Modules.Audit.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Audit.Application.Queries.GetMemberAuditLog;

public record GetMemberAuditLogQuery(Guid MemberId) : IQuery<IReadOnlyList<AuditLogResponse>>;
