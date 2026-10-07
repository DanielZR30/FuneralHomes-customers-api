using FH.Shared.Domain.Common;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Customer.Domain.Entities;

public class Customer : AggregateRoot<Guid>
{
    public CustomerType CustomerType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string IdentificationType { get; private set; } = string.Empty;
    public string IdentificationNumber { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private Customer() : base() { }

    public Customer(
        Guid id,
        CustomerType customerType,
        string name,
        string identificationType,
        string identificationNumber,
        string? email = null,
        string? phone = null,
        string? address = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del cliente es obligatorio.", nameof(name));

        if (string.IsNullOrWhiteSpace(identificationType))
            throw new ArgumentException("El tipo de identificación es obligatorio.", nameof(identificationType));

        if (string.IsNullOrWhiteSpace(identificationNumber))
            throw new ArgumentException("El número de identificación es obligatorio.", nameof(identificationNumber));

        CustomerType = customerType;
        Name = name.Trim();
        IdentificationType = identificationType.Trim().ToUpperInvariant();
        IdentificationNumber = identificationNumber.Trim().ToUpperInvariant();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        Status = CustomerStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public static Customer Create(
        CustomerType customerType,
        string name,
        string identificationType,
        string identificationNumber,
        string? email = null,
        string? phone = null,
        string? address = null)
    {
        return new Customer(
            Guid.NewGuid(),
            customerType,
            name,
            identificationType,
            identificationNumber,
            email,
            phone,
            address);
    }

    public void UpdateDemographics(string name, string? email, string? phone, string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre no puede estar vacío.", nameof(name));

        Name = name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
    }

    public void Activate() => Status = CustomerStatus.Active;
    public void Deactivate() => Status = CustomerStatus.Inactive;
    public void Suspend() => Status = CustomerStatus.Suspended;
}
