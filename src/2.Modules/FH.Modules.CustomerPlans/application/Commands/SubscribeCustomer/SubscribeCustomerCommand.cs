using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.CustomerPlans.Application.Commands.SubscribeCustomer;

public record SubscribeCustomerCommand(
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly StartDate,
    DateOnly? EndDate = null) : ICommand<SubscriptionDto>;
