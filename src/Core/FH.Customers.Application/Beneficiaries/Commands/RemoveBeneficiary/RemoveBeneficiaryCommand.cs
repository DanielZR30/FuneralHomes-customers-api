
using FH.Customers.Application.Cqrs;

namespace FH.Customers.Application.Beneficiaries.Commands.RemoveBeneficiary;

public record RemoveBeneficiaryCommand(Guid SubscriptionId, Guid MemberId) : ICommand;