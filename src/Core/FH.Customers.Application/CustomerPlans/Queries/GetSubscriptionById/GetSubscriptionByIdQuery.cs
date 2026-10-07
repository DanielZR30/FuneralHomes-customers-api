
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.DTOs;

namespace FH.Customers.Application.CustomerPlans.Queries.GetSubscriptionById;

public record GetSubscriptionByIdQuery(Guid SubscriptionId) : IQuery<SubscriptionDto>;
