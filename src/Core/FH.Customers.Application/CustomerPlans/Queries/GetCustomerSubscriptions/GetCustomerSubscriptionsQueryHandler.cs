
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Application.CustomerPlans.DTOs;

namespace FH.Customers.Application.CustomerPlans.Queries.GetCustomerSubscriptions;

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
