using FH.Modules.Beneficiaries.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Exceptions;
using FH.Shared.Domain.Repositories;

namespace FH.Modules.Beneficiaries.Application.Commands.RemoveBeneficiary;

public class RemoveBeneficiaryCommandHandler : ICommandHandler<RemoveBeneficiaryCommand>
{
    private readonly IBeneficiaryRepository _beneficiaries;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveBeneficiaryCommandHandler(
        IBeneficiaryRepository beneficiaries,
        IUnitOfWork unitOfWork)
    {
        _beneficiaries = beneficiaries;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveBeneficiaryCommand request,
        CancellationToken cancellationToken)
    {
        var beneficiary = await _beneficiaries.GetAsync(
            request.SubscriptionId,
            request.MemberId,
            cancellationToken);

        if (beneficiary is null)
        {
            return Result.NotFound("El beneficiario no se encuentra vinculado a esta suscripción.");
        }

        try
        {
            beneficiary.Remove();
        }
        catch (BusinessRuleException ex)
        {
            return Result.Conflict(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(204);
    }
}
