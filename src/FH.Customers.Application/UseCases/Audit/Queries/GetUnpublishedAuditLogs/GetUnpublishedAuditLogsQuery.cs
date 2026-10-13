using FH.Customers.Application.UseCases.Audit.Common;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Audit.Queries.GetUnpublishedAuditLogs;

public record GetUnpublishedAuditLogsQuery(int BatchSize) : IQuery<IReadOnlyList<AuditLogResponse>>;
