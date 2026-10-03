using FH.Shared.Domain.Common;
using FH.Shared.Domain.Enums;

namespace FH.Shared.Domain.Entities;

public class CustomerSubscription : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public Guid ExternalPlanId { get; private set; }
    public int MaxBeneficiaries { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public SubscriptionStatus Status { get; private set; }

    // Navigation property
    public Customer? Customer { get; private set; }

    // EF Core constructor
    private CustomerSubscription() : base() { }

    public CustomerSubscription(
        Guid id,
        Guid customerId,
        Guid externalPlanId,
        int maxBeneficiaries,
        DateOnly startDate,
        DateOnly? endDate = null) : base(id)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("El ID del cliente es obligatorio.", nameof(customerId));

        if (externalPlanId == Guid.Empty)
            throw new ArgumentException("El ID del plan externo es obligatorio.", nameof(externalPlanId));

        if (maxBeneficiaries < 1)
            throw new ArgumentException("El cupo máximo de beneficiarios debe ser al menos 1.", nameof(maxBeneficiaries));

        if (endDate.HasValue && endDate.Value < startDate)
            throw new ArgumentException("La fecha final no puede ser anterior a la inicial.", nameof(endDate));

        CustomerId = customerId;
        ExternalPlanId = externalPlanId;
        MaxBeneficiaries = maxBeneficiaries;
        StartDate = startDate;
        EndDate = endDate;
        Status = SubscriptionStatus.Active;
    }

    public static CustomerSubscription Create(
        Guid customerId,
        Guid externalPlanId,
        int maxBeneficiaries,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        return new CustomerSubscription(
            Guid.NewGuid(),
            customerId,
            externalPlanId,
            maxBeneficiaries,
            startDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            endDate);
    }

    public bool CanAcceptBeneficiary(int currentActiveBeneficiariesCount)
    {
        if (Status != SubscriptionStatus.Active)
            return false;

        return currentActiveBeneficiariesCount < MaxBeneficiaries;
    }

    public void ValidateCanAddBeneficiary(int currentActiveBeneficiariesCount)
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException($"La suscripción no está activa (Estado actual: {Status}).");

        if (!CanAcceptBeneficiary(currentActiveBeneficiariesCount))
            throw new InvalidOperationException($"Se ha alcanzado el cupo máximo de beneficiarios ({MaxBeneficiaries}) para esta suscripción.");
    }

    public void Cancel(DateOnly? cancelDate = null)
    {
        Status = SubscriptionStatus.Cancelled;
        EndDate = cancelDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
