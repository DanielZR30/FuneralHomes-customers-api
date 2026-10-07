using FH.Modules.Customer.Application.DTOs;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Customer.Application.Queries.GetCustomers;

public record GetCustomersQuery(CustomerStatus? Status = null) : IQuery<IReadOnlyList<CustomerResponse>>;
