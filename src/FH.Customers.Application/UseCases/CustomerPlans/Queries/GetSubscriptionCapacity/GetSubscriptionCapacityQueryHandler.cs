using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Domain.Common.Enums;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.CustomerPlans.Common;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.CustomerPlans.Queries.GetSubscriptionCapacity;

public class GetSubscriptionCapacityQueryHandler : IQueryHandler<GetSubscriptionCapacityQuery, SubscriptionCapacityDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IBeneficiaryRepository _beneficiaries;

    public GetSubscriptionCapacityQueryHandler(
        ISubscriptionRepository subscriptionRepository,
        IBeneficiaryRepository beneficiaries)
    {
        _subscriptionRepository = subscriptionRepository;
        _beneficiaries = beneficiaries;
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

        var assigned = await _beneficiaries.CountActiveAsync(request.SubscriptionId, cancellationToken);
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
