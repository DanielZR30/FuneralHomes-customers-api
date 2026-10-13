namespace FH.Customers.Application.UseCases.CustomerPlans.DTOs;

public record SubscriptionCapacityDto(
    Guid SubscriptionId,
    int MaxBeneficiaries,
    int AssignedBeneficiaries,
    int AvailableCapacity,
    string Status,
    bool CanAcceptBeneficiary);
