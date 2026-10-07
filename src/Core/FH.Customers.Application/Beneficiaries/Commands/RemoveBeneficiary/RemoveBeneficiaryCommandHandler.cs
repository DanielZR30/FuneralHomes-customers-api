
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Beneficiaries.Repositories;
using FH.Customers.Domain.Exceptions;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Application.Beneficiaries.Commands.RemoveBeneficiary;

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

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(204);
    }
}
