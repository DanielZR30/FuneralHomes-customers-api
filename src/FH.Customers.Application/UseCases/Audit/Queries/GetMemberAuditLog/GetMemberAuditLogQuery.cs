using FH.Customers.Application.UseCases.Audit.DTOs;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Audit.Queries.GetMemberAuditLog;

public record GetMemberAuditLogQuery(Guid MemberId) : IQuery<IReadOnlyList<AuditLogResponse>>;
