
using FH.Customers.Application.Audit.DTOs;
using FH.Customers.Application.Cqrs;

namespace FH.Customers.Application.Audit.Queries.GetUnpublishedAuditLogs;

public record GetUnpublishedAuditLogsQuery(int BatchSize) : IQuery<IReadOnlyList<AuditLogResponse>>;
