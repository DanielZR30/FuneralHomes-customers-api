using FH.Modules.Customer.Application.DTOs;
using FH.Modules.Customer.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Customer.Application.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerResponse>
{
    private readonly ICustomerRepository _customers;

    public GetCustomerByIdQueryHandler(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result<CustomerResponse>.NotFound($"El cliente {request.Id} no existe.");
        }

        return Result<CustomerResponse>.Success(CustomerResponse.FromEntity(customer));
    }
}
