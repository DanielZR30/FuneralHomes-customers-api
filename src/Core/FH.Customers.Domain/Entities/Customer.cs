
using FH.Customers.Domain.Common;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.ValueObjects;

namespace FH.Customers.Domain.Entities;

public class Customer : AggregateRoot<Guid>
{
    public CustomerType CustomerType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DocumentId Document { get; private set; } = null!;
    public ContactInfo Contact { get; private set; } = new(null, null);
    public Address? Address { get; private set; }

    // Accesos de solo lectura para quien solo necesita el texto (DTOs, consultas simples).
    public string IdentificationType => Document.Type;
    public string IdentificationNumber => Document.Number;
    public string? Email => Contact.Email;
    public string? Phone => Contact.Phone;
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

        CustomerType = customerType;
        Name = name.Trim();
        // Los value objects validan y normalizan sus propios datos.
        Document = new DocumentId(identificationType, identificationNumber);
        Contact = new ContactInfo(email, phone);
        Address = Address.FromNullable(address);
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
        Contact = new ContactInfo(email, phone);
        Address = Address.FromNullable(address);
    }

    public void Activate() => Status = CustomerStatus.Active;
    public void Deactivate() => Status = CustomerStatus.Inactive;
    public void Suspend() => Status = CustomerStatus.Suspended;
}
