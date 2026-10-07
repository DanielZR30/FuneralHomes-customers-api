
using FH.Customers.Application.Beneficiaries.DTOs;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Application.Beneficiaries.Queries.GetBeneficiariesBySubscription;

public record GetBeneficiariesBySubscriptionQuery(Guid SubscriptionId, BeneficiaryStatus? Status = null)
    : IQuery<IReadOnlyList<BeneficiaryResponse>>;