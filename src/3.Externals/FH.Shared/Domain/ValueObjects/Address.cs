using FH.Shared.Domain.Common;

namespace FH.Shared.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Value { get; }

    public Address(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Address address) => address.Value;
    public static implicit operator Address(string value) => new(value);
}
