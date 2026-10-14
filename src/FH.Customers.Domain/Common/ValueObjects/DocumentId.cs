using FH.Customers.Domain.Common;
using FH.Customers.Domain.Common.Enums;
using FH.Customers.Domain.Exceptions;

namespace FH.Customers.Domain.Common.ValueObjects;

/// <summary>
/// Documento de identidad (tipo + número). El tipo es una lista cerrada (<see cref="IdentificationType"/>)
/// y el número queda normalizado en mayúsculas.
/// Dos documentos con el mismo tipo y número son iguales (igualdad por valor).
/// </summary>
public class DocumentId : ValueObject
{
    public IdentificationType Type { get; private set; }
    public string Number { get; private set; } = string.Empty;

    /// <summary>Tipos permitidos, para mensajes de error.</summary>
    public static string AllowedTypes => string.Join(", ", Enum.GetNames<IdentificationType>());

    // Constructor para EF Core
    private DocumentId() { }

    public DocumentId(IdentificationType type, string number)
    {
        if (!Enum.IsDefined(type))
            throw new BusinessRuleException($"El tipo de identificación no es válido. Valores permitidos: {AllowedTypes}.");

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("El número de identificación es obligatorio.", nameof(number));

        Type = type;
        Number = number.Trim().ToUpperInvariant();
    }

    /// <summary>Crea el documento a partir del texto que llega en la petición (por ejemplo "cc").</summary>
    public DocumentId(string type, string number)
        : this(ParseType(type), number)
    {
    }

    /// <summary>Convierte el texto al tipo de documento sin distinguir mayúsculas. No acepta números.</summary>
    public static bool TryParseType(string? value, out IdentificationType type)
    {
        type = default;
        var text = value?.Trim();

        return !string.IsNullOrEmpty(text)
               && text.All(char.IsLetter)
               && Enum.TryParse(text, ignoreCase: true, out type);
    }

    public static bool IsValidType(string? value) => TryParseType(value, out _);

    private static IdentificationType ParseType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("El tipo de identificación es obligatorio.", nameof(type));

        if (!TryParseType(type, out var parsed))
            throw new BusinessRuleException(
                $"El tipo de identificación '{type.Trim()}' no es válido. Valores permitidos: {AllowedTypes}.");

        return parsed;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return Number;
    }

    public override string ToString() => $"{Type}:{Number}";
}
