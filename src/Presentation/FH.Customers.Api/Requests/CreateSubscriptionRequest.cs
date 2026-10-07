namespace FH.Customers.Api.Requests;

public record CreateSubscriptionRequest(
    Guid CustomerId,
    Guid ExternalPlanId,
    int MaxBeneficiaries,
    DateOnly StartDate,
    DateOnly? EndDate = null);
