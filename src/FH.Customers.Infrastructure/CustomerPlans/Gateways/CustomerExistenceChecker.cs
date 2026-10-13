using FH.Customers.Application.Contracts.Gateways;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Domain.Common.Enums;
using CustomerEntity = FH.Customers.Domain.Entities.Customer;

namespace FH.Customers.Infrastructure.CustomerPlans.Gateways;

public class CustomerExistenceChecker : ICustomerExistenceChecker
{
    private readonly IRepository<CustomerEntity, Guid> _customerRepository;

    public CustomerExistenceChecker(IRepository<CustomerEntity, Guid> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<bool> ExistsActiveAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

        return customer is not null && customer.Status == CustomerStatus.Active;
    }
}
