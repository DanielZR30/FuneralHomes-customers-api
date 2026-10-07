
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.DTOs;

namespace FH.Customers.Application.CustomerPlans.Commands.CancelSubscription;

public record CancelSubscriptionCommand(Guid SubscriptionId) : ICommand<SubscriptionDto>;
