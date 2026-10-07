
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Application.CustomerPlans.DTOs;
using FH.Customers.Domain.Exceptions;

namespace FH.Customers.Application.CustomerPlans.Commands.CancelSubscription;

public class CancelSubscriptionCommandHandler : ICommandHandler<CancelSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CancelSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
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
