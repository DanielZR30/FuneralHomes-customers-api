using FH.Shared.Domain.Common;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Services;

namespace FH.Shared.Domain.Entities;

public class Member : BaseEntity<Guid>
{
    public SubjectType SubjectType { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; }
    public string? IdentificationType { get; private set; }
    public string? IdentificationNumber { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public int DerivedAge { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

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
            throw new ArgumentException("El nombre del miembro es obligatorio.", nameof(firstName));

        SubjectType = subjectType;
        FirstName = firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
        BirthDate = birthDate;
        DerivedAge = DerivedAgeCalculator.Calculate(birthDate);

        IdentificationType = string.IsNullOrWhiteSpace(identificationType) ? null : identificationType.Trim().ToUpperInvariant();
        IdentificationNumber = string.IsNullOrWhiteSpace(identificationNumber) ? null : identificationNumber.Trim().ToUpperInvariant();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
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
}
