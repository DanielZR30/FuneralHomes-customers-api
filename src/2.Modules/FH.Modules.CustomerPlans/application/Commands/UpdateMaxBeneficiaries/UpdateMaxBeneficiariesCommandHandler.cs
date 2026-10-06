using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Modules.CustomerPlans.Domain.ValueObjects;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Exceptions;

namespace FH.Modules.CustomerPlans.Application.Commands.UpdateMaxBeneficiaries;

public class UpdateMaxBeneficiariesCommandHandler : ICommandHandler<UpdateMaxBeneficiariesCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IAssignedBeneficiariesCounter _beneficiariesCounter;
    private readonly ICustomerPlansUnitOfWork _unitOfWork;

    public UpdateMaxBeneficiariesCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IAssignedBeneficiariesCounter beneficiariesCounter,
        ICustomerPlansUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _beneficiariesCounter = beneficiariesCounter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionDto>> Handle(
        UpdateMaxBeneficiariesCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (subscription is null)
        {
            return Result<SubscriptionDto>.NotFound($"La suscripción con ID {request.SubscriptionId} no existe.");
        }

        // RN-05: El nuevo valor no puede ser menor a los beneficiarios asignados
        var assignedCount = await _beneficiariesCounter.CountActiveBeneficiariesAsync(request.SubscriptionId, cancellationToken);
        if (request.NewMax < assignedCount)
        {
            return Result<SubscriptionDto>.Conflict(
                $"El nuevo cupo ({request.NewMax}) no puede ser menor a la cantidad de beneficiarios activos asignados ({assignedCount}).");
        }

        try
        {
            subscription.UpdateMaxBeneficiaries(new MaxBeneficiaries(request.NewMax));
        }
        catch (BusinessRuleException ex)
        {
            return Result<SubscriptionDto>.Failure(ex.Message, "BusinessRule", 422);
        }

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<SubscriptionDto>.Success(SubscriptionDto.FromDomain(subscription));
    }
}
