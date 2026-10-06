using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.CustomerPlans.Application.Queries.GetCustomerSubscriptions;

public record GetCustomerSubscriptionsQuery(Guid CustomerId) : IQuery<IReadOnlyList<SubscriptionDto>>;
