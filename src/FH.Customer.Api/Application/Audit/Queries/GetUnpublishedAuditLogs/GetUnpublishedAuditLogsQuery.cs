using FH.Modules.Audit.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Audit.Application.Queries.GetUnpublishedAuditLogs;

public record GetUnpublishedAuditLogsQuery(int BatchSize) : IQuery<IReadOnlyList<AuditLogResponse>>;
