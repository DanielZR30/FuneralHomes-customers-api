using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.Common;

namespace FH.Customers.Application.UseCases.CustomerPlans.Commands.UpdateMaxBeneficiaries;

public record UpdateMaxBeneficiariesCommand(Guid SubscriptionId, int NewMax) : ICommand<SubscriptionDto>;
