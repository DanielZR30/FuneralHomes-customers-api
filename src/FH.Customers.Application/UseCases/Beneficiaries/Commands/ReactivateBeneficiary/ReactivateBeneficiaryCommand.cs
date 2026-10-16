using FH.Customers.Application.UseCases.Beneficiaries.Common;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Beneficiaries.Commands.ReactivateBeneficiary;

public record ReactivateBeneficiaryCommand(Guid SubscriptionId, Guid MemberId) : ICommand<BeneficiaryResponse>;
