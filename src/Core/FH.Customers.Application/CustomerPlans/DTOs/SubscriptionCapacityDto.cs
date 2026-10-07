using FH.Customers.Application.CustomerPlans;

namespace FH.Customers.Application.CustomerPlans.DTOs;

public record SubscriptionCapacityDto(
    Guid SubscriptionId,
    int MaxBeneficiaries,
    int AssignedBeneficiaries,
    int AvailableCapacity,
    string Status,
    bool CanAcceptBeneficiary);
