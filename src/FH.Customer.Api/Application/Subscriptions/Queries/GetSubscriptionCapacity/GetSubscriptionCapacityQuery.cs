using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.CustomerPlans.Application.Queries.GetSubscriptionCapacity;

public record GetSubscriptionCapacityQuery(Guid SubscriptionId) : IQuery<SubscriptionCapacityDto>;
