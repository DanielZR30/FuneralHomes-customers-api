using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.Customers.Common;

namespace FH.Customers.Application.UseCases.Customers.Commands.UpdateCustomerDemographics;

public record UpdateCustomerDemographicsCommand(
    Guid Id,
    string Name,
    string? Email = null,
    string? Phone = null,
    string? Address = null) : ICommand<CustomerResponse>;
