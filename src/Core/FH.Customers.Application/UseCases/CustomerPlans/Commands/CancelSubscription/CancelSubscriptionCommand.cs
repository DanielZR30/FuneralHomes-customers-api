using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.DTOs;

namespace FH.Customers.Application.UseCases.CustomerPlans.Commands.CancelSubscription;

public record CancelSubscriptionCommand(Guid SubscriptionId) : ICommand<SubscriptionDto>;
