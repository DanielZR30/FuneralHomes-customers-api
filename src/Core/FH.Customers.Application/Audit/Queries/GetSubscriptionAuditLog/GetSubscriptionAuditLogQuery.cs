
using FH.Customers.Application.Audit.DTOs;
using FH.Customers.Application.Cqrs;

namespace FH.Customers.Application.Audit.Queries.GetSubscriptionAuditLog;

public record GetSubscriptionAuditLogQuery(Guid SubscriptionId) : IQuery<IReadOnlyList<AuditLogResponse>>;
