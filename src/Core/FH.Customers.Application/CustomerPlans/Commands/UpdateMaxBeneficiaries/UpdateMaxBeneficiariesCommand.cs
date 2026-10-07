
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.DTOs;

namespace FH.Customers.Application.CustomerPlans.Commands.UpdateMaxBeneficiaries;

public record UpdateMaxBeneficiariesCommand(Guid SubscriptionId, int NewMax) : ICommand<SubscriptionDto>;
