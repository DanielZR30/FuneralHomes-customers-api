using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.CustomerPlans.Application.Commands.CancelSubscription;

public record CancelSubscriptionCommand(Guid SubscriptionId) : ICommand<SubscriptionDto>;
