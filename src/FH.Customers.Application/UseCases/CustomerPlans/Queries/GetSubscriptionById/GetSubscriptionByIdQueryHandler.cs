using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.Common;
using FH.Customers.Application.Utilities.Results;

namespace FH.Customers.Application.UseCases.CustomerPlans.Queries.GetSubscriptionById;

public class GetSubscriptionByIdQueryHandler : IQueryHandler<GetSubscriptionByIdQuery, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionByIdQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<SubscriptionDto>> Handle(
        GetSubscriptionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);
        if (subscription is null)
        {
            return Result<SubscriptionDto>.NotFound($"Suscripción con ID {request.SubscriptionId} no encontrada.");
        }

        return Result<SubscriptionDto>.Success(SubscriptionDto.FromDomain(subscription));
    }
}
