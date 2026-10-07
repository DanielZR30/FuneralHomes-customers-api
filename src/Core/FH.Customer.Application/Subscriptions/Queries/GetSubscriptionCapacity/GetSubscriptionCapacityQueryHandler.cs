using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Enums;

namespace FH.Modules.CustomerPlans.Application.Queries.GetSubscriptionCapacity;

public class GetSubscriptionCapacityQueryHandler : IQueryHandler<GetSubscriptionCapacityQuery, SubscriptionCapacityDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IAssignedBeneficiariesCounter _beneficiariesCounter;

    public GetSubscriptionCapacityQueryHandler(
        ISubscriptionRepository subscriptionRepository,
        IAssignedBeneficiariesCounter beneficiariesCounter)
    {
        _subscriptionRepository = subscriptionRepository;
        _beneficiariesCounter = beneficiariesCounter;
    }

    public async Task<Result<SubscriptionCapacityDto>> Handle(
        GetSubscriptionCapacityQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (subscription is null)
        {
            return Result<SubscriptionCapacityDto>.NotFound($"Suscripción con ID {request.SubscriptionId} no encontrada.");
        }

        var assigned = await _beneficiariesCounter.CountActiveBeneficiariesAsync(request.SubscriptionId, cancellationToken);
        var max = subscription.MaxBeneficiaries.Value;
        var available = Math.Max(0, max - assigned);
        var canAccept = subscription.Status == SubscriptionStatus.Active && available > 0;

        var dto = new SubscriptionCapacityDto(
            subscription.Id,
            max,
            assigned,
            available,
            subscription.Status.ToString().ToUpperInvariant(),
            canAccept);

        return Result<SubscriptionCapacityDto>.Success(dto);
    }
}
