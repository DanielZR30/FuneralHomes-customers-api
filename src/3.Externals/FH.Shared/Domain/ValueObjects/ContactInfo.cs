using FH.Shared.Domain.Common;

namespace FH.Shared.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public string? Email { get; }
    public string? Phone { get; }

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
