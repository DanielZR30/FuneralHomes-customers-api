using FH.Shared.Application.Cqrs;

namespace FH.Modules.Audit.Application.Commands.MarkAsPublished;

public record MarkAsPublishedCommand(Guid AuditLogId) : ICommand;
