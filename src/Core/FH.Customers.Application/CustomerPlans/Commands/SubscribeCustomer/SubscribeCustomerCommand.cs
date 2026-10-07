
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.DTOs;

namespace FH.Customers.Application.CustomerPlans.Commands.SubscribeCustomer;

public record SubscribeCustomerCommand(
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly StartDate,
    DateOnly? EndDate = null) : ICommand<SubscriptionDto>;
