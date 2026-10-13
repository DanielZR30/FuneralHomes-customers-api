using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Contracts.Persistence;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Audit.Commands.MarkAsPublished;

public class MarkAsPublishedCommandHandler : ICommandHandler<MarkAsPublishedCommand>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAsPublishedCommandHandler(
        IBeneficiaryAuditLogRepository auditLogs,
        IUnitOfWork unitOfWork)
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
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(204);
    }
}
