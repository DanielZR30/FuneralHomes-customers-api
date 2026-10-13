using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Api.DTOs.Beneficiaries;

public record AddBeneficiaryRequest(
    SubjectType SubjectType,
    string FirstName,
    DateOnly BirthDate,
    BeneficiaryType BeneficiaryType,
    RelationshipType RelationshipType,
    string? LastName = null,
    string? IdentificationType = null,
    string? IdentificationNumber = null,
    string? Email = null,
    string? Phone = null);