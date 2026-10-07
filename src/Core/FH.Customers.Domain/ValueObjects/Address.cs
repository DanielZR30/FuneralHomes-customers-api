
using FH.Customers.Domain.Common;

namespace FH.Customers.Domain.ValueObjects;

/// <summary>
/// Dirección de texto. No puede ser vacía: si no hay dirección, la propiedad del dueño es null.
/// </summary>
public class Address : ValueObject
{
    public string Value { get; private set; } = string.Empty;

    // Constructor para EF Core
    private Address() { }

    public Address(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("La dirección no puede estar vacía.", nameof(value));

        Value = value.Trim();
    }

    /// <summary>Devuelve null si el texto viene vacío; si no, la dirección.</summary>
    public static Address? FromNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : new Address(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
