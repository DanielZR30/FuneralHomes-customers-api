using FH.Modules.Customer.Application.DTOs;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Customer.Application.Commands.CreateCustomer;

public record CreateCustomerCommand(
    CustomerType CustomerType,
    string Name,
    string IdentificationType,
    string IdentificationNumber,
    string? Email = null,
    string? Phone = null,
    string? Address = null) : ICommand<CustomerResponse>;
