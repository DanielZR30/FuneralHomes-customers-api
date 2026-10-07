
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.Exceptions;

namespace FH.Customers.Domain.CustomerPlans.ValueObjects;

public record SubscriptionPeriod
{
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }

    // Constructor privado sin parámetros para EF Core
    private SubscriptionPeriod()
    {
    }

    public SubscriptionPeriod(DateOnly startDate, DateOnly? endDate = null)
    {
        if (endDate.HasValue && endDate.Value < startDate)
        {
            throw new BusinessRuleException("La fecha de finalización no puede ser anterior a la fecha de inicio.");
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    public bool IsActiveOn(DateOnly date)
    {
        if (date < StartDate)
        {
            return false;
        }

        return !EndDate.HasValue || date <= EndDate.Value;
    }

    public bool OverlapsWith(SubscriptionPeriod other)
    {
        var otherEnd = other.EndDate ?? DateOnly.MaxValue;
        var thisEnd = EndDate ?? DateOnly.MaxValue;

        return StartDate <= otherEnd && other.StartDate <= thisEnd;
    }
}
