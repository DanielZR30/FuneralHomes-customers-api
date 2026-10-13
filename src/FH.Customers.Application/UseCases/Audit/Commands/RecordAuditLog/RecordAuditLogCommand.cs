using FH.Customers.Application.UseCases.Common;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.UseCases.Audit.Commands.RecordAuditLog;

public record RecordAuditLogCommand(
    Guid SubscriptionId,
    Guid MemberId,
    AuditAction Action) : ICommand;
