using FH.Customers.Application.UseCases.Common;
using FH.Customers.Application.UseCases.Customers.DTOs;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.UseCases.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    CustomerType CustomerType,
    string Name,
    string IdentificationType,
    string IdentificationNumber,
    string? Email = null,
    string? Phone = null,
    string? Address = null) : ICommand<CustomerResponse>;
