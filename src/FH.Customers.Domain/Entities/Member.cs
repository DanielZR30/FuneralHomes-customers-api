
using FH.Customers.Domain.Common;
using FH.Customers.Domain.Common.Enums;
using FH.Customers.Domain.Common.ValueObjects;
using FH.Customers.Domain.Exceptions;


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
        DerivedAge = CalculateAge(birthDate);

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
        DerivedAge = CalculateAge(BirthDate, asOfDate);
    }

    // Solo se modifica a través de su raíz: Beneficiary.UpdateMemberDetails.
    internal void UpdateDetails(
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
        var derivedAge = CalculateAge(birthDate);
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

    private static int CalculateAge(DateOnly birthDate, DateOnly? asOfDate = null)
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
