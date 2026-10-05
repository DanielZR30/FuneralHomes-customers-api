using FH.Modules.Customer.Application.DTOs;
using FH.Modules.Customer.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Customer.Application.Queries.GetCustomers;

public class GetCustomersQueryHandler
    : IQueryHandler<GetCustomersQuery, IReadOnlyList<CustomerResponse>>
{
    private readonly ICustomerRepository _customers;

    public GetCustomersQueryHandler(ICustomerRepository customers)
    {
        _customers = customers;
    }

    public async Task<Result<IReadOnlyList<CustomerResponse>>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var customers = request.Status is null
            ? await _customers.GetAllAsync(cancellationToken)
            : await _customers.FindAsync(
                c => c.Status == request.Status.Value,
                cancellationToken);

        IReadOnlyList<CustomerResponse> response = customers
            .OrderBy(c => c.Name)
            .Select(CustomerResponse.FromEntity)
            .ToList();

        return Result<IReadOnlyList<CustomerResponse>>.Success(response);
    }
}
