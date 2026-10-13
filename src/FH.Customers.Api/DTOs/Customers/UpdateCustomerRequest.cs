namespace FH.Customers.Api.DTOs.Customers;

public record UpdateCustomerRequest(
    string Name,
    string? Email,
    string? Phone,
    string? Address);