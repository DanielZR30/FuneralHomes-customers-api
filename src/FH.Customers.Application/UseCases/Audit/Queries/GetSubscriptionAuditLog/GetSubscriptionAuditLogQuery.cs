using FH.Customers.Application.UseCases.Audit.DTOs;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Audit.Queries.GetSubscriptionAuditLog;

public record GetSubscriptionAuditLogQuery(Guid SubscriptionId) : IQuery<IReadOnlyList<AuditLogResponse>>;
