
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Application.Audit.Commands.RecordAuditLog;

public record RecordAuditLogCommand(
    Guid SubscriptionId,
    Guid MemberId,
    AuditAction Action) : ICommand;
