
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.Customers.DTOs;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Application.Customers.Queries.GetCustomers;

public record GetCustomersQuery(CustomerStatus? Status = null) : IQuery<IReadOnlyList<CustomerResponse>>;
