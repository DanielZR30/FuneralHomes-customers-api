
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Beneficiaries.DTOs;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Beneficiaries.Repositories;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Exceptions;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Application.Beneficiaries.Commands.AddBeneficiary;

public class AddBeneficiaryCommandHandler : ICommandHandler<AddBeneficiaryCommand, BeneficiaryResponse>
{
    private readonly IRepository<Subscription, Guid> _subscriptions;
    private readonly IBeneficiaryRepository _beneficiaries;
    private readonly IUnitOfWork _unitOfWork;

    public AddBeneficiaryCommandHandler(
        IRepository<Subscription, Guid> subscriptions,
        IBeneficiaryRepository beneficiaries,
        IUnitOfWork unitOfWork)
    {
        _subscriptions = subscriptions;
        _beneficiaries = beneficiaries;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BeneficiaryResponse>> Handle(
        AddBeneficiaryCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptions.GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result<BeneficiaryResponse>.NotFound(
                $"La suscripción {request.SubscriptionId} no existe.");
        }

        if (subscription.Status != SubscriptionStatus.Active)
        {
            return Result<BeneficiaryResponse>.UnprocessableEntity(
                $"La suscripción no está activa (Estado: {subscription.Status}).");
        }

        var activeCount = await _beneficiaries.CountActiveAsync(request.SubscriptionId, cancellationToken);

        if (!subscription.CanAcceptBeneficiary(activeCount))
        {
            return Result<BeneficiaryResponse>.Conflict(
                $"Se ha alcanzado el cupo máximo de beneficiarios ({subscription.MaxBeneficiaries.Value}) para esta suscripción. Activos: {activeCount}.");
        }

        Member member;
        Beneficiary beneficiary;

        try
        {
            member = Member.Create(
                request.SubjectType,
                request.FirstName,
                request.BirthDate,
                request.LastName,
                request.IdentificationType,
                request.IdentificationNumber,
                request.Email,
                request.Phone);

            beneficiary = Beneficiary.Enroll(
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
        await _unitOfWork.CommitAsync(cancellationToken);

        var response = BeneficiaryResponse.FromEntity(beneficiary, member);

        return Result<BeneficiaryResponse>.Success(response, 201);
    }
}