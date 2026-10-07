
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Domain.CustomerPlans;

namespace FH.Customers.Application.CustomerPlans.DTOs;

public record SubscriptionDto(
    Guid Id,
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status)
{
    public static SubscriptionDto FromDomain(Subscription subscription) =>
        new(
            subscription.Id,
            subscription.CustomerId,
            subscription.ExternalPlanId,
            subscription.MaxBeneficiaries.Value,
            subscription.Period.StartDate,
            subscription.Period.EndDate,
            subscription.Status.ToString().ToUpperInvariant());
}
