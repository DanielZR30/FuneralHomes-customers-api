using FH.Modules.Beneficiaries.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Beneficiaries.Application.Commands.UpdateBeneficiary;

public record UpdateBeneficiaryCommand(
    Guid SubscriptionId,
    Guid MemberId,
    string FirstName,
    DateOnly BirthDate,
    string? LastName = null,
    string? IdentificationType = null,
    string? IdentificationNumber = null,
    string? Email = null,
    string? Phone = null) : ICommand<BeneficiaryResponse>;