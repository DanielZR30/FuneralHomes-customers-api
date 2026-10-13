using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.Common;

namespace FH.Customers.Application.UseCases.CustomerPlans.Commands.CancelSubscription;

public record CancelSubscriptionCommand(Guid SubscriptionId) : ICommand<SubscriptionDto>;
