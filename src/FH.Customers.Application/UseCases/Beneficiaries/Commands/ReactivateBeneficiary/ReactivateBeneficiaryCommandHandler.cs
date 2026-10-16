using FH.Customers.Domain.Common.Enums;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Exceptions;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Contracts.Persistence;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.Beneficiaries.Common;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Beneficiaries.Commands.ReactivateBeneficiary;

public class ReactivateBeneficiaryCommandHandler : ICommandHandler<ReactivateBeneficiaryCommand, BeneficiaryResponse>
{
    private readonly IRepository<Subscription, Guid> _subscriptions;
    private readonly IBeneficiaryRepository _beneficiaries;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateBeneficiaryCommandHandler(
        IRepository<Subscription, Guid> subscriptions,
        IBeneficiaryRepository beneficiaries,
        IUnitOfWork unitOfWork)
    {
        _subscriptions = subscriptions;
        _beneficiaries = beneficiaries;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BeneficiaryResponse>> Handle(
        ReactivateBeneficiaryCommand request,
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

        // 1) Regla del dominio: solo se reincorpora un beneficiario retirado.
        //    El cambio queda en memoria; si alguna revisión de abajo falla, no se hace commit y se descarta.
        try
        {
            beneficiary.Reactivate();
        }
        catch (BusinessRuleException ex)
        {
            return Result<BeneficiaryResponse>.Conflict(ex.Message);
        }

        // 2) La suscripción debe estar activa y con cupo (las mismas reglas del alta).
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

        // 3) Mientras estuvo retirado, otra persona con el mismo documento pudo quedar activa.
        var member = beneficiary.Member!;

        if (member.SubjectType == SubjectType.Human
            && member.IdentificationType is not null
            && member.IdentificationNumber is not null)
        {
            var alreadyEnrolled = await _beneficiaries.ExistsActiveByIdentificationAsync(
                request.SubscriptionId,
                member.IdentificationType,
                member.IdentificationNumber,
                cancellationToken,
                excludeMemberId: member.Id);

            if (alreadyEnrolled)
            {
                return Result<BeneficiaryResponse>.Conflict(
                    "Ya existe un beneficiario activo con ese documento en esta suscripción.");
            }
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<BeneficiaryResponse>.Success(BeneficiaryResponse.FromEntity(beneficiary, member));
    }
}
