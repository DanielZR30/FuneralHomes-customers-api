namespace FH.Customers.Application.Contracts.Gateways;

public interface IAssignedBeneficiariesCounter
{
    /// <summary>
    /// Obtiene la cantidad de beneficiarios activos actualmente asignados a la suscripción (RN-05).
    /// </summary>
    Task<int> CountActiveBeneficiariesAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
