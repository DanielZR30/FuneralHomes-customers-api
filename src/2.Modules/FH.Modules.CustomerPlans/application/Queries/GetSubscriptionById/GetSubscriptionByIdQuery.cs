using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.CustomerPlans.Application.Queries.GetSubscriptionById;

public record GetSubscriptionByIdQuery(Guid SubscriptionId) : IQuery<SubscriptionDto>;
