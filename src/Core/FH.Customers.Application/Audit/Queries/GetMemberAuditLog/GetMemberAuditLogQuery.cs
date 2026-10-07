
using FH.Customers.Application.Audit.DTOs;
using FH.Customers.Application.Cqrs;

namespace FH.Customers.Application.Audit.Queries.GetMemberAuditLog;

public record GetMemberAuditLogQuery(Guid MemberId) : IQuery<IReadOnlyList<AuditLogResponse>>;
