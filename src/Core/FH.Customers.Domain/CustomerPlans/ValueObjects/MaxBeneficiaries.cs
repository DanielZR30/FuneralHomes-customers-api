
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.Exceptions;

namespace FH.Customers.Domain.CustomerPlans.ValueObjects;

public record MaxBeneficiaries
{
    public int Value { get; }

    public MaxBeneficiaries(int value)
    {
        if (value < 1)
        {
            throw new BusinessRuleException("El cupo máximo de beneficiarios debe ser mayor o igual a 1.");
        }

        Value = value;
    }

    public static implicit operator int(MaxBeneficiaries maxBeneficiaries) => maxBeneficiaries.Value;
    public static implicit operator MaxBeneficiaries(int value) => new(value);

    public override string ToString() => Value.ToString();
}
