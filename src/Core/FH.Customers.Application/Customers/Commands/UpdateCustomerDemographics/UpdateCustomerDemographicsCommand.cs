
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.Customers.DTOs;

namespace FH.Customers.Application.Customers.Commands.UpdateCustomerDemographics;

public record UpdateCustomerDemographicsCommand(
    Guid Id,
    string Name,
    string? Email = null,
    string? Phone = null,
    string? Address = null) : ICommand<CustomerResponse>;
