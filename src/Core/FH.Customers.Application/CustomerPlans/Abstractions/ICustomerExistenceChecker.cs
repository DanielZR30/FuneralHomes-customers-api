using FH.Customers.Application.CustomerPlans;

namespace FH.Customers.Application.CustomerPlans.Abstractions;

public interface ICustomerExistenceChecker
{
    /// <summary>
    /// Verifica si un cliente existe y se encuentra en estado activo en el sistema (RN-06).
    /// </summary>
    Task<bool> ExistsActiveAsync(Guid customerId, CancellationToken cancellationToken = default);
}
