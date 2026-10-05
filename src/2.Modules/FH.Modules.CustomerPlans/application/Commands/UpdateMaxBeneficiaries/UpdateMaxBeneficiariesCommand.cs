using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.CustomerPlans.Application.Commands.UpdateMaxBeneficiaries;

public record UpdateMaxBeneficiariesCommand(Guid SubscriptionId, int NewMax) : ICommand<SubscriptionDto>;
