using FH.Shared.Domain.Common;

namespace FH.Shared.Domain.ValueObjects;

public class DocumentId : ValueObject
{
    public string Type { get; }
    public string Number { get; }

    public DocumentId(string type, string number)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("El tipo de identificación es requerido.", nameof(type));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("El número de identificación es requerido.", nameof(number));

        Type = type.Trim().ToUpperInvariant();
        Number = number.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Number;
    }

    public override string ToString() => $"{Type}:{Number}";
}
