
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Beneficiaries.DTOs;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Beneficiaries.Repositories;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Exceptions;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Application.Beneficiaries.Commands.UpdateBeneficiary;

public class UpdateBeneficiaryCommandHandler : ICommandHandler<UpdateBeneficiaryCommand, BeneficiaryResponse>
{
    private readonly IBeneficiaryRepository _beneficiaries;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBeneficiaryCommandHandler(
        IBeneficiaryRepository beneficiaries,
        IUnitOfWork unitOfWork)
    {
        _beneficiaries = beneficiaries;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BeneficiaryResponse>> Handle(
        UpdateBeneficiaryCommand request,
        CancellationToken cancellationToken)
    {
        var beneficiary = await _beneficiaries.GetAsync(
            request.SubscriptionId,
            request.MemberId,
            cancellationToken);

        if (beneficiary is null)
        {
            return Result<BeneficiaryResponse>.NotFound(
                "El beneficiario no se encuentra vinculado a esta suscripción.");
        }

        if (beneficiary.Status != BeneficiaryStatus.Active)
        {
            return Result<BeneficiaryResponse>.Conflict(
                "Solo se pueden modificar los datos de un beneficiario activo.");
        }

        try
        {
            beneficiary.UpdateMemberDetails(
                request.FirstName,
                request.BirthDate,
                request.LastName,
                request.IdentificationType,
                request.IdentificationNumber,
                request.Email,
                request.Phone);
        }
        catch (BusinessRuleException ex)
        {
            return Result<BeneficiaryResponse>.Failure(ex.Message, "BusinessRule", 400);
        }

        var member = beneficiary.Member!;

        if (member.SubjectType == SubjectType.Human
            && member.IdentificationType is not null
            && member.IdentificationNumber is not null)
        {
            var duplicated = await _beneficiaries.ExistsActiveByIdentificationAsync(
                request.SubscriptionId,
                member.IdentificationType,
                member.IdentificationNumber,
                cancellationToken,
                excludeMemberId: member.Id);

            if (duplicated)
            {
                return Result<BeneficiaryResponse>.Conflict(
                    "Ya existe otro beneficiario activo con ese documento en esta suscripción.");
            }
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<BeneficiaryResponse>.Success(BeneficiaryResponse.FromEntity(beneficiary, member));
    }
}