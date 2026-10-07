
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.Customers.DTOs;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    CustomerType CustomerType,
    string Name,
    string IdentificationType,
    string IdentificationNumber,
    string? Email = null,
    string? Phone = null,
    string? Address = null) : ICommand<CustomerResponse>;
