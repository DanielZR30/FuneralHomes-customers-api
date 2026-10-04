using FH.Modules.Beneficiaries.Application.DTOs;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Beneficiaries.Application.Commands.AddBeneficiary;

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