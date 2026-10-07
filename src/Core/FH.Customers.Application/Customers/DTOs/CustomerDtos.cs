
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Application.Customers.DTOs;

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
    public static CustomerResponse FromEntity(FH.Customers.Domain.Entities.Customer customer) =>
        new(
            customer.Id,
            customer.CustomerType.ToString(),
            customer.Name,
            customer.IdentificationType,
            customer.IdentificationNumber,
            customer.Email,
            customer.Phone,
            customer.Address?.Value,
            customer.Status.ToString(),
            customer.CreatedAt);
}
