using FH.Shared.Domain.Entities;

namespace FH.Modules.CustomerPlans.Application.DTOs;

public record CreateSubscriptionRequest(
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly? StartDate,
    DateOnly? EndDate);

public record SubscriptionResponse(
    Guid Id,
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Status)
{
    public static SubscriptionResponse FromEntity(CustomerSubscription sub) =>
        new(
            sub.Id,
            sub.CustomerId,
            sub.ExternalPlanId,
            sub.MaxBeneficiaries,
            sub.StartDate,
            sub.EndDate,
            sub.Status.ToString());
}
