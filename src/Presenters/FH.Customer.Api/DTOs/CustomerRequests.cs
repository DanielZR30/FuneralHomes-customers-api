using FH.Shared.Domain.Enums;

namespace FH.Customer.Api.Controllers;

public record CreateCustomerRequest(
    CustomerType CustomerType,
    string Name,
    string IdentificationType,
    string IdentificationNumber,
    string Email,
    string Phone,
    string Address);

public record UpdateCustomerDemographicsRequest(
    string Name,
    string Email,
    string Phone,
    string Address);

public record ChangeCustomerStatusRequest(
    CustomerStatus NewStatus);
