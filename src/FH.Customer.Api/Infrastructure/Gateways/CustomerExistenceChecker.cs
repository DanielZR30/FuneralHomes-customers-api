using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Repositories;
using FH.Shared.MockData;

namespace FH.Modules.CustomerPlans.Infrastructure.Gateways;

public class CustomerExistenceChecker : ICustomerExistenceChecker
{
    private readonly IRepository<FH.Shared.Domain.Entities.Customer, Guid>? _customerRepository;
    private readonly InMemoryCustomerStore? _mockStore;

    public CustomerExistenceChecker(
        IRepository<FH.Shared.Domain.Entities.Customer, Guid>? customerRepository = null,
        InMemoryCustomerStore? mockStore = null)
    {
        _customerRepository = customerRepository;
        _mockStore = mockStore;
    }

    public async Task<bool> ExistsActiveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        // 1. Verificar primero en el repositorio si está disponible
        if (_customerRepository is not null)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
                if (customer is not null)
                {
                    return customer.Status == CustomerStatus.Active;
                }
            }
            catch
            {
                // Si la base de datos no está disponible en entorno de desarrollo/mock, continuar con fallback
            }
        }

        // 2. Fallback a la memoria (para desarrollo y pruebas locales)
        if (_mockStore is not null)
        {
            var customer = _mockStore.GetCustomerById(customerId);
            return customer is not null && customer.Status == CustomerStatus.Active;
        }

        return false;
    }
}
