using FH.Shared.Application.Cqrs;

namespace FH.Modules.Beneficiaries.Application.Commands.RemoveBeneficiary;

public record RemoveBeneficiaryCommand(Guid SubscriptionId, Guid MemberId) : ICommand;