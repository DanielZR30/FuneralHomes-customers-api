using FH.Customers.Domain.Common.Enums;
using FH.Customers.Domain.Entities;

namespace FH.Customers.Application.UseCases.Customers.DTOs;

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
    public static CustomerResponse FromEntity(Customer customer) =>
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
