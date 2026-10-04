using FH.Modules.Beneficiaries.Application.DTOs;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Beneficiaries.Application.Queries.GetBeneficiariesBySubscription;

public record GetBeneficiariesBySubscriptionQuery(Guid SubscriptionId, BeneficiaryStatus? Status = null)
    : IQuery<IReadOnlyList<BeneficiaryResponse>>;