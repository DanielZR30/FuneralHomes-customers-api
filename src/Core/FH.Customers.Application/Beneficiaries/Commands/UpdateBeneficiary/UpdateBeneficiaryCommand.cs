
using FH.Customers.Application.Beneficiaries.DTOs;
using FH.Customers.Application.Cqrs;

namespace FH.Customers.Application.Beneficiaries.Commands.UpdateBeneficiary;

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