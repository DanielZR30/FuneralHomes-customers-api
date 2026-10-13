using FH.Customers.Application.UseCases.Beneficiaries.Common;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Beneficiaries.Commands.UpdateBeneficiary;

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