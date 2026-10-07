using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Repositories;

namespace FH.Modules.Audit.Application.Commands.RecordAuditLog;

public class RecordAuditLogCommandHandler : ICommandHandler<RecordAuditLogCommand>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;
    private readonly IUnitOfWork _unitOfWork;

    public RecordAuditLogCommandHandler(
        IBeneficiaryAuditLogRepository auditLogs,
        IUnitOfWork unitOfWork)
    {
        _auditLogs = auditLogs;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RecordAuditLogCommand request, CancellationToken cancellationToken)
    {
        if (request.SubscriptionId == Guid.Empty || request.MemberId == Guid.Empty)
        {
            return Result.Failure("El identificador de suscripción y el de miembro son obligatorios.");
        }

        var log = BeneficiaryAuditLog.Create(request.SubscriptionId, request.MemberId, request.Action);

        await _auditLogs.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(201);
    }
}
