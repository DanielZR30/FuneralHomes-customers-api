using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Domain.Customers.Repositories;

public interface ICustomerRepository : IRepository<CustomerEntity, Guid>
{
    /// <summary>
    /// Indica si ya existe un cliente con la pareja tipo + número de identificación.
    /// La pareja completa es la clave de negocio: un mismo número puede existir
    /// bajo tipos de documento distintos (por ejemplo CC y NIT).
    /// </summary>
    Task<bool> ExistsByIdentificationAsync(
        string identificationType,
        string identificationNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el cliente registrado con la pareja tipo + número de identificación.
    /// </summary>
    Task<CustomerEntity?> GetByIdentificationAsync(
        string identificationType,
        string identificationNumber,
        CancellationToken cancellationToken = default);
}
