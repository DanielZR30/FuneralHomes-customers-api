using FH.Shared.Domain.Enums;
using CustomerEntity = FH.Modules.Customer.Domain.Entities.Customer;

namespace FH.Modules.Customer.Tests.Domain;

public class CustomerEntityTests
{
    [Fact]
    public void Create_WithValidData_ShouldBuildActiveCustomer()
    {
        var customer = CustomerEntity.Create(
            CustomerType.Individual,
            "Ana María Gómez",
            "CC",
            "1010101010",
            "ANA@CORREO.COM",
            "3001234567",
            "Calle 1 #2-3");

        Assert.Equal(CustomerType.Individual, customer.CustomerType);
        Assert.Equal(CustomerStatus.Active, customer.Status);
        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.NotEqual(default, customer.CreatedAt);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // RN-01: el nombre del titular es obligatorio
        Assert.Throws<ArgumentException>(() =>
            CustomerEntity.Create(CustomerType.Individual, "   ", "CC", "1010101010"));
    }

    [Fact]
    public void Create_WithBlankIdentification_ShouldThrowArgumentException()
    {
        // RN-02: la identificación es obligatoria en sus dos partes
        Assert.Throws<ArgumentException>(() =>
            CustomerEntity.Create(CustomerType.Individual, "Ana", "  ", "1010101010"));

        Assert.Throws<ArgumentException>(() =>
            CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "  "));
    }

    [Fact]
    public void Create_ShouldNormalizeIdentificationAndEmail()
    {
        var customer = CustomerEntity.Create(
            CustomerType.Corporate,
            "  funeraria del norte ltda  ",
            "  nit  ",
            "  900123456  ",
            "  CONTACTO@FUNERARIA.COM  ");

        // El nombre solo se recorta: la entidad no altera el casing del texto del usuario.
        Assert.Equal("funeraria del norte ltda", customer.Name);
        Assert.Equal("NIT", customer.IdentificationType);
        Assert.Equal("900123456", customer.IdentificationNumber);
        Assert.Equal("contacto@funeraria.com", customer.Email);
    }

    [Fact]
    public void Create_WithBlankOptionalFields_ShouldStoreNull()
    {
        var customer = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1", "  ", "  ", "  ");

        Assert.Null(customer.Email);
        Assert.Null(customer.Phone);
        Assert.Null(customer.Address);
    }

    [Fact]
    public void UpdateDemographics_WithBlankName_ShouldThrowArgumentException()
    {
        var customer = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1010101010");

        // RN-04: el nombre no puede quedar vacío
        Assert.Throws<ArgumentException>(() =>
            customer.UpdateDemographics("   ", "nuevo@correo.com", null, null));
    }

    [Fact]
    public void UpdateDemographics_ShouldNotAlterIdentification()
    {
        // RN-05: el documento de identificación es inmutable
        var customer = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1010101010");

        customer.UpdateDemographics("Ana María Gómez", "nuevo@correo.com", "3009999999", "Av. Siempre Viva 742");

        Assert.Equal("CC", customer.IdentificationType);
        Assert.Equal("1010101010", customer.IdentificationNumber);
        Assert.Equal("Ana María Gómez", customer.Name);
        Assert.Equal("nuevo@correo.com", customer.Email);
    }

    [Fact]
    public void UpdateDemographics_WithNullOptionalFields_ShouldClearThem()
    {
        var customer = CustomerEntity.Create(
            CustomerType.Individual, "Ana", "CC", "1010101010", "ana@correo.com", "3001234567", "Calle 1");

        customer.UpdateDemographics("Ana", null, null, null);

        Assert.Null(customer.Email);
        Assert.Null(customer.Phone);
        Assert.Null(customer.Address);
    }

    [Theory]
    [InlineData(CustomerStatus.Active)]
    [InlineData(CustomerStatus.Inactive)]
    [InlineData(CustomerStatus.Suspended)]
    public void ActivateDeactivateSuspend_ShouldSetRequestedStatus(CustomerStatus expected)
    {
        var customer = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1010101010");

        switch (expected)
        {
            case CustomerStatus.Active:
                customer.Activate();
                break;
            case CustomerStatus.Inactive:
                customer.Deactivate();
                break;
            case CustomerStatus.Suspended:
                customer.Suspend();
                break;
        }

        Assert.Equal(expected, customer.Status);
    }
}
