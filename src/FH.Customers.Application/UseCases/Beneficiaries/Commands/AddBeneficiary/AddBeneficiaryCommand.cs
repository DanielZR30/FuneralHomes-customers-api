using FH.Customers.Application.UseCases.Beneficiaries.DTOs;
using FH.Customers.Application.UseCases.Common;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.UseCases.Beneficiaries.Commands.AddBeneficiary;

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