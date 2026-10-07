
using FH.Customers.Application.Beneficiaries.DTOs;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Beneficiaries.Repositories;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Application.Beneficiaries.Queries.GetBeneficiariesBySubscription;

public class GetBeneficiariesBySubscriptionQueryHandler
    : IQueryHandler<GetBeneficiariesBySubscriptionQuery, IReadOnlyList<BeneficiaryResponse>>
{
    private readonly IRepository<Subscription, Guid> _subscriptions;
    private readonly IBeneficiaryRepository _beneficiaries;

    public GetBeneficiariesBySubscriptionQueryHandler(
        IRepository<Subscription, Guid> subscriptions,
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