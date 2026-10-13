using FH.Customers.Application.UseCases.Beneficiaries.Common;
using FH.Customers.Application.UseCases.Common;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.UseCases.Beneficiaries.Queries.GetBeneficiariesBySubscription;

public record GetBeneficiariesBySubscriptionQuery(Guid SubscriptionId, BeneficiaryStatus? Status = null)
    : IQuery<IReadOnlyList<BeneficiaryResponse>>;