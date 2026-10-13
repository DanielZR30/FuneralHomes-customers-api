using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.Customers.Common;

namespace FH.Customers.Application.UseCases.Customers.Queries.GetCustomerByIdentification;

public record GetCustomerByIdentificationQuery(string Type, string Number) : IQuery<CustomerResponse>;
