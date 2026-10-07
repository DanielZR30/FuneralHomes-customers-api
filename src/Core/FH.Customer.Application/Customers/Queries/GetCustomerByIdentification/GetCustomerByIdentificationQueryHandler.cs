using FH.Modules.Customer.Application.DTOs;
using FH.Modules.Customer.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Customer.Application.Queries.GetCustomerByIdentification;

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
        // RN-08: ambos componentes del documento son obligatorios para consultar.
        if (string.IsNullOrWhiteSpace(request.Type) || string.IsNullOrWhiteSpace(request.Number))
        {
            return Result<CustomerResponse>.Failure(
                "El tipo y el número de identificación son obligatorios.", "Validation", 400);
        }

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
