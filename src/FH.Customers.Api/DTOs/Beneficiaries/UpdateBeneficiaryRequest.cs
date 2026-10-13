namespace FH.Customers.Api.DTOs.Beneficiaries;

public record UpdateBeneficiaryRequest(
    string FirstName,
    DateOnly BirthDate,
    string? LastName = null,
    string? IdentificationType = null,
    string? IdentificationNumber = null,
    string? Email = null,
    string? Phone = null);