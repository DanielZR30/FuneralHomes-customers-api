using CustomerEntity = FH.Modules.Customer.Domain.Entities.Customer;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Customer.Application.DTOs;

public record CreateCustomerRequest(
    CustomerType CustomerType,
    string Name,
    string IdentificationType,
    string IdentificationNumber,
    string? Email,
    string? Phone,
    string? Address);

public record UpdateCustomerRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Address);

public record ChangeCustomerStatusRequest(CustomerStatus Status);

public record CustomerResponse(
    Guid Id,
    string CustomerType,
    string Name,
    string IdentificationType,
    string IdentificationNumber,
    string? Email,
    string? Phone,
    string? Address,
    string Status,
    DateTime CreatedAt)
{
    public static CustomerResponse FromEntity(CustomerEntity customer) =>
        new(
            customer.Id,
            customer.CustomerType.ToString(),
            customer.Name,
            customer.IdentificationType,
            customer.IdentificationNumber,
            customer.Email,
            customer.Phone,
            customer.Address,
            customer.Status.ToString(),
            customer.CreatedAt);
}
