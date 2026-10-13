using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Api.DTOs.Customers;

public record CreateCustomerRequest(
    CustomerType CustomerType,
    string Name,
    string IdentificationType,
    string IdentificationNumber,
    string? Email,
    string? Phone,
    string? Address);