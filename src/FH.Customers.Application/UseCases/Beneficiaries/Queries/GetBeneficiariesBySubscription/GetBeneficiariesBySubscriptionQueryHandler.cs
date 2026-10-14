using FH.Customers.Domain.Entities;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.Beneficiaries.Common;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Beneficiaries.Queries.GetBeneficiariesBySubscription;

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

        // La edad derivada se guarda al crear o actualizar el miembro; se recalcula al consultar
        // para que no quede desactualizada (no se guarda: esta consulta no hace commit).
        foreach (var beneficiary in beneficiaries)
        {
            beneficiary.Member!.RefreshDerivedAge();
        }

        IReadOnlyList<BeneficiaryResponse> response = beneficiaries
            .Select(b => BeneficiaryResponse.FromEntity(b, b.Member!))
            .ToList();

        return Result<IReadOnlyList<BeneficiaryResponse>>.Success(response);
    }
}