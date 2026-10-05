namespace FH.Modules.CustomerPlans.Application.DTOs;

public record SubscriptionCapacityDto(
    Guid SubscriptionId,
    int MaxBeneficiaries,
    int AssignedBeneficiaries,
    int AvailableCapacity,
    string Status,
    bool CanAcceptBeneficiary);
