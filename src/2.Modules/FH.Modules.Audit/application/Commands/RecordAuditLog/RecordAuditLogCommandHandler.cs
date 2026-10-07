using FH.Modules.Audit.Application.Abstractions;
using FH.Modules.Audit.Domain.Entities;
using FH.Modules.Audit.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Audit.Application.Commands.RecordAuditLog;

public class RecordAuditLogCommandHandler : ICommandHandler<RecordAuditLogCommand>
{
    private readonly IBeneficiaryAuditLogRepository _auditLogs;
    private readonly IAuditUnitOfWork _unitOfWork;

    public RecordAuditLogCommandHandler(
        IBeneficiaryAuditLogRepository auditLogs,
        IAuditUnitOfWork unitOfWork)
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
