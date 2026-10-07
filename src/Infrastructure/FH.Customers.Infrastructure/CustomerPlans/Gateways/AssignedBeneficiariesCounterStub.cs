
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;

namespace FH.Customers.Infrastructure.CustomerPlans.Gateways;

/// <summary>
/// Implementación por defecto del contador de beneficiarios asignados (RN-05).
/// TODO: Pendiente de integración con el Módulo 3 (Beneficiarios).
/// </summary>
public class AssignedBeneficiariesCounterStub : IAssignedBeneficiariesCounter
{
    public Task<int> CountActiveBeneficiariesAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        // Retorna 0 como valor por defecto hasta que se complete la integración con Módulo 3
        return Task.FromResult(0);
    }
}
