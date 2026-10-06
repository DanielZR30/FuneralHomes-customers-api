using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Exceptions;

namespace FH.Modules.CustomerPlans.Application.Commands.CancelSubscription;

public class CancelSubscriptionCommandHandler : ICommandHandler<CancelSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICustomerPlansUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CancelSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        ICustomerPlansUnitOfWork unitOfWork,
        TimeProvider? timeProvider = null)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<Result<SubscriptionDto>> Handle(
        CancelSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (subscription is null)
        {
            return Result<SubscriptionDto>.NotFound($"La suscripción con ID {request.SubscriptionId} no existe.");
        }

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        try
        {
            subscription.CancelSubscription(today);
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
