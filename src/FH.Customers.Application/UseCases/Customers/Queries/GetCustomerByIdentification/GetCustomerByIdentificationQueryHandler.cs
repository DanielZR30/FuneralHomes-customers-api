using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Application.Utilities.Results;
using FH.Customers.Application.UseCases.Customers.Common;
using FH.Customers.Application.UseCases.Common;

namespace FH.Customers.Application.UseCases.Customers.Queries.GetCustomerByIdentification;

public class GetCustomerByIdentificationQueryHandler
    : IQueryHandler<GetCustomerByIdentificationQuery, CustomerResponse>
{
    private readonly ICustomerRepository _customers;

    public GetCustomerByIdentificationQueryHandler(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerByIdentificationQuery request,
        CancellationToken cancellationToken)
    {
        // RN-08 (ambos componentes del documento obligatorios) se valida en GetCustomerByIdentificationQueryValidator.
        var customer = await _customers.GetByIdentificationAsync(
            request.Type,
            request.Number,
            cancellationToken);

        if (customer is null)
        {
            return Result<CustomerResponse>.NotFound(
                $"No existe un cliente con la identificación {request.Type.Trim().ToUpperInvariant()} {request.Number.Trim().ToUpperInvariant()}.");
        }

        return Result<CustomerResponse>.Success(CustomerResponse.FromEntity(customer));
    }
}
