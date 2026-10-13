using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.CustomerPlans.Common;

namespace FH.Customers.Application.UseCases.CustomerPlans.Commands.SubscribeCustomer;

public record SubscribeCustomerCommand(
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly StartDate,
    DateOnly? EndDate = null) : ICommand<SubscriptionDto>;
