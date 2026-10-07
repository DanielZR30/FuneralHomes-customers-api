
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.Customers.DTOs;

namespace FH.Customers.Application.Customers.Queries.GetCustomerByIdentification;

public record GetCustomerByIdentificationQuery(string Type, string Number) : IQuery<CustomerResponse>;
