using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.Customers.DTOs;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.UseCases.Customers.Queries.GetCustomers;

public record GetCustomersQuery(CustomerStatus? Status = null) : IQuery<IReadOnlyList<CustomerResponse>>;
