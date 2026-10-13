using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.DTOs;

namespace FH.Customers.Application.UseCases.CustomerPlans.Queries.GetCustomerSubscriptions;

public record GetCustomerSubscriptionsQuery(Guid CustomerId) : IQuery<IReadOnlyList<SubscriptionDto>>;
