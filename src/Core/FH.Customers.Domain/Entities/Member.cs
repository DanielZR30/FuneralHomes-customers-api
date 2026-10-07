
using FH.Customers.Domain.Common;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Exceptions;
using FH.Customers.Domain.Services;
using FH.Customers.Domain.ValueObjects;

namespace FH.Customers.Domain.Entities;

public class Member : BaseEntity<Guid>
{
    public SubjectType SubjectType { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; }
    public DocumentId? Document { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public int DerivedAge { get; private set; }
    public ContactInfo Contact { get; private set; } = new(null, null);

    // Accesos de solo lectura para quien solo necesita el texto.
    public string? IdentificationType => Document?.Type;
    public string? IdentificationNumber => Document?.Number;
    public string? Email => Contact.Email;
    public string? Phone => Contact.Phone;

    // EF Core constructor
    private Member() : base() { }

    public Member(
        Guid id,
        SubjectType subjectType,
        string firstName,
        DateOnly birthDate,
        string? lastName = null,
        string? identificationType = null,
        string? identificationNumber = null,
        string? email = null,
        string? phone = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleException("El nombre del miembro es obligatorio.");

        SubjectType = subjectType;
        FirstName = firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
        BirthDate = birthDate;
        DerivedAge = DerivedAgeCalculator.Calculate(birthDate);

        Document = BuildDocument(identificationType, identificationNumber);
        Contact = new ContactInfo(email, phone);
    }

    public static Member Create(
        SubjectType subjectType,
        string firstName,
        DateOnly birthDate,
        string? lastName = null,
        string? identificationType = null,
        string? identificationNumber = null,
        string? email = null,
        string? phone = null)
    {
        return new Member(
            Guid.NewGuid(),
            subjectType,
            firstName,
            birthDate,
            lastName,
            identificationType,
            identificationNumber,
            email,
            phone);
    }

    public void RefreshDerivedAge(DateOnly? asOfDate = null)
    {
        DerivedAge = DerivedAgeCalculator.Calculate(BirthDate, asOfDate);
    }

    public void UpdateDetails(
    string firstName,
    DateOnly birthDate,
    string? lastName = null,
    string? identificationType = null,
    string? identificationNumber = null,
    string? email = null,
    string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessRuleException("El nombre del miembro es obligatorio.");

        // Se calcula primero: si la fecha es futura, falla antes de modificar nada.
        var derivedAge = DerivedAgeCalculator.Calculate(birthDate);
        var document = BuildDocument(identificationType, identificationNumber);

        FirstName = firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
        BirthDate = birthDate;
        DerivedAge = derivedAge;
        Document = document;
        Contact = new ContactInfo(email, phone);
    }

    // El documento es opcional, pero si se informa debe venir completo (tipo + número).
    private static DocumentId? BuildDocument(string? type, string? number)
    {
        var hasType = !string.IsNullOrWhiteSpace(type);
        var hasNumber = !string.IsNullOrWhiteSpace(number);

        if (!hasType && !hasNumber)
            return null;

        if (hasType != hasNumber)
            throw new BusinessRuleException("El tipo y el número de identificación deben informarse juntos.");

        return new DocumentId(type!, number!);
    }
}
