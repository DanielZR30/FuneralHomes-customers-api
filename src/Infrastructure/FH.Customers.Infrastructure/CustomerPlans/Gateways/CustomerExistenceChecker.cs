using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Repositories;
using FH.Customers.Infrastructure.MockData;

namespace FH.Customers.Infrastructure.CustomerPlans.Gateways;

public class CustomerExistenceChecker : ICustomerExistenceChecker
{
    private readonly IRepository<CustomerEntity, Guid>? _customerRepository;
    private readonly InMemoryCustomerStore? _mockStore;

    public CustomerExistenceChecker(
        IRepository<CustomerEntity, Guid>? customerRepository = null,
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
