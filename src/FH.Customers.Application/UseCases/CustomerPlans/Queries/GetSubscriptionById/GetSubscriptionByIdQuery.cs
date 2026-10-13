using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.Common;

namespace FH.Customers.Application.UseCases.CustomerPlans.Queries.GetSubscriptionById;

public record GetSubscriptionByIdQuery(Guid SubscriptionId) : IQuery<SubscriptionDto>;
