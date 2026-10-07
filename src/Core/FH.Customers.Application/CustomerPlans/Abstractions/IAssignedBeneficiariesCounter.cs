using FH.Customers.Application.CustomerPlans;

namespace FH.Customers.Application.CustomerPlans.Abstractions;

public interface IAssignedBeneficiariesCounter
{
    /// <summary>
    /// Obtiene la cantidad de beneficiarios activos actualmente asignados a la suscripción (RN-05).
    /// </summary>
    Task<int> CountActiveBeneficiariesAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
