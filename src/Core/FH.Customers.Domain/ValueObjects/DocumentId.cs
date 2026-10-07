
using FH.Customers.Domain.Common;

namespace FH.Customers.Domain.ValueObjects;

/// <summary>
/// Documento de identidad (tipo + número). Siempre queda normalizado en mayúsculas.
/// Dos documentos con el mismo tipo y número son iguales (igualdad por valor).
/// </summary>
public class DocumentId : ValueObject
{
    public string Type { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;

    // Constructor para EF Core
    private DocumentId() { }

    public DocumentId(string type, string number)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("El tipo de identificación es obligatorio.", nameof(type));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("El número de identificación es obligatorio.", nameof(number));

        Type = type.Trim().ToUpperInvariant();
        Number = number.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Number;
    }

    public override string ToString() => $"{Type}:{Number}";
}
