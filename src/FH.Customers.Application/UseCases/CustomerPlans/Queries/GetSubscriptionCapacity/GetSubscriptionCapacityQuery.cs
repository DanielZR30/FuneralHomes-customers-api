using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.DTOs;

namespace FH.Customers.Application.UseCases.CustomerPlans.Queries.GetSubscriptionCapacity;

public record GetSubscriptionCapacityQuery(Guid SubscriptionId) : IQuery<SubscriptionCapacityDto>;
