using FH.Modules.Audit.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Audit.Application.Queries.GetSubscriptionAuditLog;

public record GetSubscriptionAuditLogQuery(Guid SubscriptionId) : IQuery<IReadOnlyList<AuditLogResponse>>;
