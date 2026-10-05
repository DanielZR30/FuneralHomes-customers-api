using FH.Modules.CustomerPlans.Domain.Events;
using FH.Modules.CustomerPlans.Domain.ValueObjects;
using FH.Shared.Domain.Common;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Exceptions;

namespace FH.Modules.CustomerPlans.Domain;

public sealed class Subscription : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public Guid ExternalPlanId { get; private set; }
    public MaxBeneficiaries MaxBeneficiaries { get; private set; } = null!;
    public SubscriptionPeriod Period { get; private set; } = null!;
    public SubscriptionStatus Status { get; private set; }

    // Constructor privado sin parámetros para EF Core
    private Subscription() : base()
    {
    }

    private Subscription(
        Guid id,
        Guid customerId,
        Guid externalPlanId,
        MaxBeneficiaries maxBeneficiaries,
        SubscriptionPeriod period) : base(id)
    {
        if (customerId == Guid.Empty)
        {
            throw new BusinessRuleException("El ID del cliente no puede ser vacío.");
        }

        if (externalPlanId == Guid.Empty)
        {
            throw new BusinessRuleException("El ID del plan externo no puede ser vacío.");
        }

        CustomerId = customerId;
        ExternalPlanId = externalPlanId;
        MaxBeneficiaries = maxBeneficiaries ?? throw new BusinessRuleException("El cupo de beneficiarios es obligatorio.");
        Period = period ?? throw new BusinessRuleException("El período de suscripción es obligatorio.");
        Status = SubscriptionStatus.Active;
    }

    public static Subscription Create(
        Guid customerId,
        Guid externalPlanId,
        MaxBeneficiaries maxBeneficiaries,
        DateOnly startDate,
        DateOnly? endDate = null)
    {
        var id = Guid.NewGuid();
        var period = new SubscriptionPeriod(startDate, endDate);
        var subscription = new Subscription(id, customerId, externalPlanId, maxBeneficiaries, period);

        subscription.AddDomainEvent(new SubscriptionCreated(
            id,
            customerId,
            externalPlanId,
            maxBeneficiaries.Value,
            startDate,
            endDate));

        return subscription;
    }

    public void CancelSubscription(DateOnly todayUtc)
    {
        if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired)
        {
            throw new BusinessRuleException($"No se puede cancelar una suscripción en estado {Status.ToString().ToUpperInvariant()}.");
        }

        var effectiveEndDate = todayUtc > Period.StartDate ? todayUtc : Period.StartDate;
        Period = new SubscriptionPeriod(Period.StartDate, effectiveEndDate);
        Status = SubscriptionStatus.Cancelled;

        AddDomainEvent(new SubscriptionCancelled(Id, CustomerId, effectiveEndDate));
    }

    public void UpdateMaxBeneficiaries(MaxBeneficiaries newMax)
    {
        if (Status != SubscriptionStatus.Active)
        {
            throw new BusinessRuleException($"Solo se puede actualizar el cupo de una suscripción activa (estado actual: {Status.ToString().ToUpperInvariant()}).");
        }

        if (newMax is null)
        {
            throw new BusinessRuleException("El nuevo cupo de beneficiarios no puede ser nulo.");
        }

        var previousMax = MaxBeneficiaries.Value;
        MaxBeneficiaries = newMax;

        AddDomainEvent(new MaxBeneficiariesUpdated(Id, previousMax, newMax.Value));
    }

    public void Suspend()
    {
        if (Status != SubscriptionStatus.Active)
        {
            throw new BusinessRuleException($"Solo se puede suspender una suscripción activa (estado actual: {Status.ToString().ToUpperInvariant()}).");
        }

        Status = SubscriptionStatus.Suspended;
    }

    public void Reactivate()
    {
        if (Status != SubscriptionStatus.Suspended)
        {
            throw new BusinessRuleException($"Solo se puede reactivar una suscripción suspendida (estado actual: {Status.ToString().ToUpperInvariant()}).");
        }

        Status = SubscriptionStatus.Active;
    }

    public void Expire(DateOnly todayUtc)
    {
        if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired)
        {
            throw new BusinessRuleException($"No se puede expirar una suscripción en estado {Status.ToString().ToUpperInvariant()}.");
        }

        var effectiveEndDate = todayUtc > Period.StartDate ? todayUtc : Period.StartDate;
        Period = new SubscriptionPeriod(Period.StartDate, effectiveEndDate);
        Status = SubscriptionStatus.Expired;
    }
}
