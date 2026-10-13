using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.Common;
using FH.Customers.Application.Utilities.Results;

namespace FH.Customers.Application.UseCases.CustomerPlans.Queries.GetCustomerSubscriptions;

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
