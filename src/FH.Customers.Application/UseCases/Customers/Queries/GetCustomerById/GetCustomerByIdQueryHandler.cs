using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.Customers.Common;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Customers.Queries.GetCustomerById;

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
