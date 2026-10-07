
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.DTOs;

namespace FH.Customers.Application.CustomerPlans.Queries.GetSubscriptionCapacity;

public record GetSubscriptionCapacityQuery(Guid SubscriptionId) : IQuery<SubscriptionCapacityDto>;
