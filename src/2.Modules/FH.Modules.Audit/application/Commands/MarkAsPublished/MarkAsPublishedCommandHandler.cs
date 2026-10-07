using FH.Modules.Audit.Application.Abstractions;
using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Audit.Application.Commands.MarkAsPublished;

public class MarkAsPublishedCommandHandler : ICommandHandler<MarkAsPublishedCommand>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;
    private readonly IAuditUnitOfWork _unitOfWork;

    public MarkAsPublishedCommandHandler(
        IBeneficiaryAuditLogRepository auditLogs,
        IAuditUnitOfWork unitOfWork)
    {
        _auditLogs = auditLogs;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkAsPublishedCommand request, CancellationToken cancellationToken)
    {
        if (request.AuditLogId == Guid.Empty)
        {
            return Result.NotFound("El registro de auditoría no existe.");
        }

        var log = await _auditLogs.GetByIdAsync(request.AuditLogId, cancellationToken);

        if (log is null)
        {
            return Result.NotFound("El registro de auditoría no existe.");
        }

        log.MarkEventPublished();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(204);
    }
}
