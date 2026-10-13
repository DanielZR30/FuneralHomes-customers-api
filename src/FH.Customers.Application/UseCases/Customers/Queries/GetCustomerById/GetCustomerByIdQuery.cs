using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.Customers.Common;

namespace FH.Customers.Application.UseCases.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerResponse>;
