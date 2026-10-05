using FH.Modules.CustomerPlans.Domain;

namespace FH.Modules.CustomerPlans.Application.DTOs;

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
