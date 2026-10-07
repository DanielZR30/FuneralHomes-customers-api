
using FH.Customers.Application.Beneficiaries.Commands.AddBeneficiary;
using FH.Customers.Application.Beneficiaries.Commands.UpdateBeneficiary;
using FH.Customers.Application.Customers.Commands.CreateCustomer;
using FH.Customers.Application.Customers.Commands.UpdateCustomerDemographics;
using FH.Customers.Application.Customers.Queries.GetCustomerByIdentification;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Tests.Application;

/// <summary>
/// Las reglas de forma de las peticiones viven en validadores FluentValidation
/// (los ejecuta el mediador antes de llamar al handler).
/// </summary>
public class ValidatorsTests
{
    // ---------- CreateCustomer (RN-01, RN-02) ----------

    [Fact]
    public void CreateCustomer_WhenValid_ShouldPass()
    {
        var result = new CreateCustomerCommandValidator().Validate(
            new CreateCustomerCommand(CustomerType.Individual, "Ana", "CC", "1010101010", "ana@example.com"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateCustomer_WhenNameIsBlank_ShouldFail()
    {
        var result = new CreateCustomerCommandValidator().Validate(
            new CreateCustomerCommand(CustomerType.Individual, "   ", "CC", "1010101010"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Theory]
    [InlineData("", "1010101010", "IdentificationType")]
    [InlineData("CC", "   ", "IdentificationNumber")]
    public void CreateCustomer_WhenIdentificationIsIncomplete_ShouldFail(string type, string number, string property)
    {
        var result = new CreateCustomerCommandValidator().Validate(
            new CreateCustomerCommand(CustomerType.Individual, "Ana", type, number));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == property);
    }

    [Fact]
    public void CreateCustomer_WhenEmailIsMalformed_ShouldFail()
    {
        var result = new CreateCustomerCommandValidator().Validate(
            new CreateCustomerCommand(CustomerType.Individual, "Ana", "CC", "1", "no-es-un-correo"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    // ---------- UpdateCustomerDemographics (RN-04) ----------

    [Fact]
    public void UpdateCustomer_WhenNameIsBlank_ShouldFail()
    {
        var result = new UpdateCustomerDemographicsCommandValidator().Validate(
            new UpdateCustomerDemographicsCommand(Guid.NewGuid(), "   "));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    // ---------- GetCustomerByIdentification (RN-08) ----------

    [Theory]
    [InlineData("", "999")]
    [InlineData("CC", "   ")]
    public void GetByIdentification_WhenArgumentsAreBlank_ShouldFail(string type, string number)
    {
        var result = new GetCustomerByIdentificationQueryValidator().Validate(
            new GetCustomerByIdentificationQuery(type, number));

        Assert.False(result.IsValid);
    }

    // ---------- Beneficiaries ----------

    [Fact]
    public void AddBeneficiary_WhenValid_ShouldPass()
    {
        var result = new AddBeneficiaryCommandValidator().Validate(
            new AddBeneficiaryCommand(
                Guid.NewGuid(), SubjectType.Human, "Laura", new DateOnly(1990, 5, 10),
                BeneficiaryType.Associated, RelationshipType.Spouse, "Gómez", "CC", "123", "laura@example.com"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void AddBeneficiary_WhenBirthDateIsInTheFuture_ShouldFail()
    {
        var result = new AddBeneficiaryCommandValidator().Validate(
            new AddBeneficiaryCommand(
                Guid.NewGuid(), SubjectType.Human, "Laura", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                BeneficiaryType.Associated, RelationshipType.Spouse));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BirthDate");
    }

    [Fact]
    public void AddBeneficiary_WhenOnlyDocumentTypeIsGiven_ShouldFail()
    {
        var result = new AddBeneficiaryCommandValidator().Validate(
            new AddBeneficiaryCommand(
                Guid.NewGuid(), SubjectType.Human, "Laura", new DateOnly(1990, 5, 10),
                BeneficiaryType.Associated, RelationshipType.Spouse, IdentificationType: "CC"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IdentificationNumber");
    }

    [Fact]
    public void UpdateBeneficiary_WhenFirstNameIsBlank_ShouldFail()
    {
        var result = new UpdateBeneficiaryCommandValidator().Validate(
            new UpdateBeneficiaryCommand(Guid.NewGuid(), Guid.NewGuid(), " ", new DateOnly(1990, 5, 10)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FirstName");
    }
}
