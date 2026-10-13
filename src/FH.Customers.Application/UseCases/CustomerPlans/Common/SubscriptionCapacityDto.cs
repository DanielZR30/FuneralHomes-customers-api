namespace FH.Customers.Application.UseCases.CustomerPlans.Common;

public record SubscriptionCapacityDto(
    Guid SubscriptionId,
    int MaxBeneficiaries,
    int AssignedBeneficiaries,
    int AvailableCapacity,
    string Status,
    bool CanAcceptBeneficiary);
