using FH.Shared.Domain.Exceptions;

namespace FH.Shared.Domain.Services;

public static class DerivedAgeCalculator
{
    public static int Calculate(DateOnly birthDate, DateOnly? asOfDate = null)
    {
        var targetDate = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        if (birthDate > targetDate)
            throw new BusinessRuleException("La fecha de nacimiento no puede ser posterior a la fecha actual.");

        var age = targetDate.Year - birthDate.Year;

        if (targetDate.Month < birthDate.Month ||
            (targetDate.Month == birthDate.Month && targetDate.Day < birthDate.Day))
        {
            age--;
        }

        return Math.Max(0, age);
    }
}
