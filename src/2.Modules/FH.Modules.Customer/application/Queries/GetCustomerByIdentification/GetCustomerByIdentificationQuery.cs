using FH.Modules.Customer.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Customer.Application.Queries.GetCustomerByIdentification;

public record GetCustomerByIdentificationQuery(string Type, string Number) : IQuery<CustomerResponse>;
