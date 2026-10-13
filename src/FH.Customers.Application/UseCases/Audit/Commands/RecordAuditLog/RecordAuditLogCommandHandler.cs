using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Domain.Entities;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Contracts.Persistence;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Audit.Commands.RecordAuditLog;

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
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(201);
    }
}
