using FH.Modules.Beneficiaries.Application.Abstractions;
using FH.Modules.Beneficiaries.Application.DTOs;
using FH.Modules.Beneficiaries.Domain.Abstractions;
using FH.Modules.Beneficiaries.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using BeneficiaryEntity = FH.Modules.Beneficiaries.Domain.Entities.Beneficiary;
using MemberEntity = FH.Modules.Beneficiaries.Domain.Entities.Member;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Exceptions;

namespace FH.Modules.Beneficiaries.Application.Commands.AddBeneficiary;

public class AddBeneficiaryCommandHandler : ICommandHandler<AddBeneficiaryCommand, BeneficiaryResponse>
{
    private readonly ISubscriptionChecker _subscriptionChecker;
    private readonly IBeneficiaryRepository _beneficiaries;
    private readonly IBeneficiariesUnitOfWork _unitOfWork;

    public AddBeneficiaryCommandHandler(
        ISubscriptionChecker subscriptionChecker,
        IBeneficiaryRepository beneficiaries,
        IBeneficiariesUnitOfWork unitOfWork)
    {
        _subscriptionChecker = subscriptionChecker;
        _beneficiaries = beneficiaries;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BeneficiaryResponse>> Handle(
        AddBeneficiaryCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionChecker.GetSubscriptionAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result<BeneficiaryResponse>.NotFound(
                $"La suscripción {request.SubscriptionId} no existe.");
        }

        if (!subscription.IsActive)
        {
            return Result<BeneficiaryResponse>.UnprocessableEntity(
                $"La suscripción no está activa.");
        }

        var activeCount = await _beneficiaries.CountActiveAsync(request.SubscriptionId, cancellationToken);

        if (!subscription.CanAcceptBeneficiary(activeCount))
        {
            return Result<BeneficiaryResponse>.Conflict(
                $"Se ha alcanzado el cupo máximo de beneficiarios ({subscription.MaxBeneficiaries}) para esta suscripción. Activos: {activeCount}.");
        }

        MemberEntity member;
        BeneficiaryEntity beneficiary;

        try
        {
            member = MemberEntity.Create(
                request.SubjectType,
                request.FirstName,
                request.BirthDate,
                request.LastName,
                request.IdentificationType,
                request.IdentificationNumber,
                request.Email,
                request.Phone);

            beneficiary = BeneficiaryEntity.Enroll(
                request.SubscriptionId,
                member,
                request.BeneficiaryType,
                request.RelationshipType);
        }
        catch (BusinessRuleException ex)
        {
            return Result<BeneficiaryResponse>.Failure(ex.Message, "BusinessRule", 400);
        }

        if (member.SubjectType == SubjectType.Human
            && member.IdentificationType is not null
            && member.IdentificationNumber is not null)
        {
            var alreadyEnrolled = await _beneficiaries.ExistsActiveByIdentificationAsync(
                request.SubscriptionId,
                member.IdentificationType,
                member.IdentificationNumber,
                cancellationToken);

            if (alreadyEnrolled)
            {
                return Result<BeneficiaryResponse>.Conflict(
                    "Ya existe un beneficiario activo con ese documento en esta suscripción.");
            }
        }

        await _beneficiaries.AddAsync(member, beneficiary, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = BeneficiaryResponse.FromEntity(beneficiary, member);

        return Result<BeneficiaryResponse>.Success(response, 201);
    }
}