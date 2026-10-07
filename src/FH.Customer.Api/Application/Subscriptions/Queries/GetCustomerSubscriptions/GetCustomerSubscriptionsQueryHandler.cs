using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.CustomerPlans.Application.Queries.GetCustomerSubscriptions;

public class GetCustomerSubscriptionsQueryHandler : IQueryHandler<GetCustomerSubscriptionsQuery, IReadOnlyList<SubscriptionDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetCustomerSubscriptionsQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<IReadOnlyList<SubscriptionDto>>> Handle(
        GetCustomerSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var dtos = subscriptions.Select(SubscriptionDto.FromDomain).ToList();

        return Result<IReadOnlyList<SubscriptionDto>>.Success(dtos);
    }
}
