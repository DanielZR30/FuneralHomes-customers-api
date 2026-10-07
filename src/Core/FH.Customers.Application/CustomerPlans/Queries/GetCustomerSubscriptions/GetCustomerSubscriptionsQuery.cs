
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.DTOs;

namespace FH.Customers.Application.CustomerPlans.Queries.GetCustomerSubscriptions;

public record GetCustomerSubscriptionsQuery(Guid CustomerId) : IQuery<IReadOnlyList<SubscriptionDto>>;
