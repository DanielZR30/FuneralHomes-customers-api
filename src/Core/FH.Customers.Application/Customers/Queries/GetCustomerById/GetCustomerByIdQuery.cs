
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.Customers.DTOs;

namespace FH.Customers.Application.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerResponse>;
