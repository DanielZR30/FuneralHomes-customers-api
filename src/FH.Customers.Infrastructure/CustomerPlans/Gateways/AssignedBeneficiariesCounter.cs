using FH.Customers.Application.Contracts.Gateways;
using FH.Customers.Application.Contracts.Repositories;

namespace FH.Customers.Infrastructure.CustomerPlans.Gateways;

/// <summary>
/// Cuenta los beneficiarios activos de una suscripción (RN-05) usando el repositorio de beneficiarios.
/// Reemplaza al stub que devolvía 0 mientras el módulo de Beneficiarios no existía.
/// </summary>
public class AssignedBeneficiariesCounter : IAssignedBeneficiariesCounter
{
    private readonly IBeneficiaryRepository _beneficiaries;

    public AssignedBeneficiariesCounter(IBeneficiaryRepository beneficiaries)
    {
        _beneficiaries = beneficiaries;
    }

    public Task<int> CountActiveBeneficiariesAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return _beneficiaries.CountActiveAsync(subscriptionId, cancellationToken);
    }
}
