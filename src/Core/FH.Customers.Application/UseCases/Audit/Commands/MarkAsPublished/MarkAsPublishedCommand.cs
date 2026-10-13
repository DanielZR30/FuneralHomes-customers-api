using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Audit.Commands.MarkAsPublished;

public record MarkAsPublishedCommand(Guid AuditLogId) : ICommand;
