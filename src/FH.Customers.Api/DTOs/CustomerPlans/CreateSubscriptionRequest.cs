namespace FH.Customers.Api.DTOs.CustomerPlans;

public record CreateSubscriptionRequest(
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly StartDate,
    DateOnly? EndDate = null);
