using FH.Modules.Customer.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Customer.Application.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerResponse>;
