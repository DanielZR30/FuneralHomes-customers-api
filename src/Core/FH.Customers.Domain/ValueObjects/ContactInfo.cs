
using FH.Customers.Domain.Common;

namespace FH.Customers.Domain.ValueObjects;

/// <summary>
/// Datos de contacto (correo y teléfono, ambos opcionales). El correo se guarda en minúsculas.
/// </summary>
public class ContactInfo : ValueObject
{
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    // Constructor para EF Core
    private ContactInfo() { }

    public ContactInfo(string? email, string? phone)
    {
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
    }
}
