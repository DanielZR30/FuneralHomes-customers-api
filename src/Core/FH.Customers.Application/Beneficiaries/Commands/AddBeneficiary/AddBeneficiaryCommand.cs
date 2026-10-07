
using FH.Customers.Application.Beneficiaries.DTOs;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Application.Beneficiaries.Commands.AddBeneficiary;

public record AddBeneficiaryCommand(
    Guid SubscriptionId,
    SubjectType SubjectType,
    string FirstName,
    DateOnly BirthDate,
    BeneficiaryType BeneficiaryType,
    RelationshipType RelationshipType,
    string? LastName = null,
    string? IdentificationType = null,
    string? IdentificationNumber = null,
    string? Email = null,
    string? Phone = null) : ICommand<BeneficiaryResponse>;