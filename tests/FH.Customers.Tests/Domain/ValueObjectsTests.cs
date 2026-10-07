using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Exceptions;
using FH.Customers.Domain.ValueObjects;

namespace FH.Customers.Tests.Domain;

public class ValueObjectsTests
{
    // ---------- DocumentId ----------

    [Fact]
    public void DocumentId_ShouldNormalizeTypeAndNumberToUpperCase()
    {
        var doc = new DocumentId(" cc ", " ab123 ");

        Assert.Equal("CC", doc.Type);
        Assert.Equal("AB123", doc.Number);
    }

    [Fact]
    public void DocumentId_WithSameValues_ShouldBeEqual()
    {
        Assert.Equal(new DocumentId("cc", "1"), new DocumentId("CC", "1"));
        Assert.True(new DocumentId("CC", "1") == new DocumentId("cc", "1"));
        Assert.NotEqual(new DocumentId("CC", "1"), new DocumentId("NIT", "1"));
    }

    [Theory]
    [InlineData("", "123")]
    [InlineData("CC", " ")]
    public void DocumentId_WithBlankData_ShouldThrow(string type, string number)
    {
        Assert.Throws<ArgumentException>(() => new DocumentId(type, number));
    }

    // ---------- ContactInfo ----------

    [Fact]
    public void ContactInfo_ShouldLowerCaseEmailAndTrimPhone()
    {
        var contact = new ContactInfo(" ANA@Correo.COM ", " 300 ");

        Assert.Equal("ana@correo.com", contact.Email);
        Assert.Equal("300", contact.Phone);
    }

    [Fact]
    public void ContactInfo_WithBlankValues_ShouldStoreNull()
    {
        var contact = new ContactInfo("  ", "");

        Assert.Null(contact.Email);
        Assert.Null(contact.Phone);
    }

    [Fact]
    public void ContactInfo_WithSameValues_ShouldBeEqual()
    {
        Assert.Equal(new ContactInfo("a@b.com", "1"), new ContactInfo("A@B.com", "1"));
    }

    // ---------- Address ----------

    [Fact]
    public void Address_ShouldTrimValue()
    {
        Assert.Equal("Calle 1", new Address("  Calle 1 ").Value);
    }

    [Fact]
    public void Address_WithBlankValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => new Address("  "));
    }

    [Fact]
    public void Address_FromNullable_WithBlank_ShouldReturnNull()
    {
        Assert.Null(Address.FromNullable(null));
        Assert.Null(Address.FromNullable("   "));
        Assert.Equal("Calle 1", Address.FromNullable("Calle 1")!.Value);
    }

    // ---------- Uso dentro de las entidades ----------

    [Fact]
    public void Customer_ShouldBuildValueObjectsFromRawData()
    {
        var customer = CustomerEntity.Create(
            CustomerType.Individual, "Ana", "cc", "1010", "ANA@X.COM", "300", "Calle 1");

        Assert.Equal(new DocumentId("CC", "1010"), customer.Document);
        Assert.Equal(new ContactInfo("ana@x.com", "300"), customer.Contact);
        Assert.Equal(new Address("Calle 1"), customer.Address);
    }

    [Fact]
    public void Customer_WithoutAddress_ShouldHaveNullAddress()
    {
        var customer = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1010");

        Assert.Null(customer.Address);
        Assert.NotNull(customer.Contact);
    }

    [Fact]
    public void Member_WithoutDocument_ShouldHaveNullDocument()
    {
        var member = Member.Create(SubjectType.Pet, "Firulais", new DateOnly(2020, 1, 1));

        Assert.Null(member.Document);
        Assert.Null(member.IdentificationType);
        Assert.Null(member.IdentificationNumber);
    }

    [Fact]
    public void Member_WithCompleteDocument_ShouldBuildDocumentId()
    {
        var member = Member.Create(
            SubjectType.Human, "Luis", new DateOnly(1990, 1, 1),
            identificationType: "cc", identificationNumber: "55");

        Assert.Equal(new DocumentId("CC", "55"), member.Document);
    }

    [Fact]
    public void Member_WithOnlyDocumentType_ShouldThrowBusinessRule()
    {
        Assert.Throws<BusinessRuleException>(() => Member.Create(
            SubjectType.Human, "Luis", new DateOnly(1990, 1, 1),
            identificationType: "CC"));
    }
}
