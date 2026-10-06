using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Audit.Application.Commands.RecordAuditLog;

public record RecordAuditLogCommand(
    Guid SubscriptionId,
    Guid MemberId,
    AuditAction Action) : ICommand;
