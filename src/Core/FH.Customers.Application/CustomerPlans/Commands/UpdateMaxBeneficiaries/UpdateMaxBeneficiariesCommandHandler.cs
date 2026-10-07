
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Application.CustomerPlans.DTOs;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.CustomerPlans.ValueObjects;
using FH.Customers.Domain.Exceptions;

namespace FH.Customers.Application.CustomerPlans.Commands.UpdateMaxBeneficiaries;

public class UpdateMaxBeneficiariesCommandHandler : ICommandHandler<UpdateMaxBeneficiariesCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IAssignedBeneficiariesCounter _beneficiariesCounter;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMaxBeneficiariesCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IAssignedBeneficiariesCounter beneficiariesCounter,
        IUnitOfWork unitOfWork)
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
