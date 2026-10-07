using FH.Modules.Beneficiaries.Application.DTOs;
using FH.Modules.Beneficiaries.Domain.Abstractions;
using FH.Modules.Beneficiaries.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Beneficiaries.Application.Queries.GetBeneficiariesBySubscription;

public class GetBeneficiariesBySubscriptionQueryHandler
    : IQueryHandler<GetBeneficiariesBySubscriptionQuery, IReadOnlyList<BeneficiaryResponse>>
{
    private readonly ISubscriptionChecker _subscriptionChecker;
    private readonly IBeneficiaryRepository _beneficiaries;

    public GetBeneficiariesBySubscriptionQueryHandler(
        ISubscriptionChecker subscriptionChecker,
        IBeneficiaryRepository beneficiaries)
    {
        _subscriptionChecker = subscriptionChecker;
        _beneficiaries = beneficiaries;
    }

    public async Task<Result<IReadOnlyList<BeneficiaryResponse>>> Handle(
        GetBeneficiariesBySubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionChecker.GetSubscriptionAsync(request.SubscriptionId, cancellationToken);

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