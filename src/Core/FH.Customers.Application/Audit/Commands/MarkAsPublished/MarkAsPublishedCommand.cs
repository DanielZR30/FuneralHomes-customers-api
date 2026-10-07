
using FH.Customers.Application.Cqrs;

namespace FH.Customers.Application.Audit.Commands.MarkAsPublished;

public record MarkAsPublishedCommand(Guid AuditLogId) : ICommand;
