using FH.Modules.Customer.Application.DTOs;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Customer.Application.Commands.UpdateCustomerDemographics;

public record UpdateCustomerDemographicsCommand(
    Guid Id,
    string Name,
    string? Email = null,
    string? Phone = null,
    string? Address = null) : ICommand<CustomerResponse>;
