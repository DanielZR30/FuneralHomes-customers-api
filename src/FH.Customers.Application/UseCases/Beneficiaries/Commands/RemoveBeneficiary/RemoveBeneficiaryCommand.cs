using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Beneficiaries.Commands.RemoveBeneficiary;

public record RemoveBeneficiaryCommand(Guid SubscriptionId, Guid MemberId) : ICommand;