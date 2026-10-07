
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.Customers.DTOs;
using FH.Customers.Domain.Customers.Repositories;

namespace FH.Customers.Application.Customers.Queries.GetCustomers;

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
