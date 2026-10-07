using FH.Modules.Beneficiaries.Application.DTOs;
using FH.Modules.Beneficiaries.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Repositories;

namespace FH.Modules.Beneficiaries.Application.Queries.GetBeneficiariesBySubscription;

public class GetBeneficiariesBySubscriptionQueryHandler
    : IQueryHandler<GetBeneficiariesBySubscriptionQuery, IReadOnlyList<BeneficiaryResponse>>
{
    private readonly FH.Modules.CustomerPlans.Application.Abstractions.ISubscriptionRepository _subscriptions;
    private readonly IBeneficiaryRepository _beneficiaries;

    public GetBeneficiariesBySubscriptionQueryHandler(
        FH.Modules.CustomerPlans.Application.Abstractions.ISubscriptionRepository subscriptions,
        IBeneficiaryRepository beneficiaries)
    {
        _subscriptions = subscriptions;
        _beneficiaries = beneficiaries;
    }

    public async Task<Result<IReadOnlyList<BeneficiaryResponse>>> Handle(
        GetBeneficiariesBySubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptions.GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return Result<IReadOnlyList<BeneficiaryResponse>>.NotFound(
                $"La suscripción {request.SubscriptionId} no existe.");
        }

        var beneficiaries = await _beneficiaries.GetBySubscriptionAsync(
            request.SubscriptionId,
            request.Status,
            cancellationToken);

        IReadOnlyList<BeneficiaryResponse> response = beneficiaries
            .Select(b => BeneficiaryResponse.FromEntity(b, b.Member!))
            .ToList();

        return Result<IReadOnlyList<BeneficiaryResponse>>.Success(response);
    }
}