using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Api.DTOs.Customers;

public record ChangeCustomerStatusRequest(CustomerStatus Status);