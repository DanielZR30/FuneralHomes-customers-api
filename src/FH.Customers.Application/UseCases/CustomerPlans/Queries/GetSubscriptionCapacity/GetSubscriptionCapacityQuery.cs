using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.Common;

namespace FH.Customers.Application.UseCases.CustomerPlans.Queries.GetSubscriptionCapacity;

public record GetSubscriptionCapacityQuery(Guid SubscriptionId) : IQuery<SubscriptionCapacityDto>;
