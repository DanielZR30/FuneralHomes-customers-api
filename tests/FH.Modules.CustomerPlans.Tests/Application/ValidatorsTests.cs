using FH.Modules.CustomerPlans.Application.Commands.CancelSubscription;
using FH.Modules.CustomerPlans.Application.Commands.SubscribeCustomer;
using FH.Modules.CustomerPlans.Application.Commands.UpdateMaxBeneficiaries;
using Xunit;

namespace FH.Modules.CustomerPlans.Tests.Application;

public class ValidatorsTests
{
    [Fact]
    public void SubscribeCustomerCommandValidator_WhenInvalid_ShouldHaveValidationErrors()
    {
        var validator = new SubscribeCustomerCommandValidator();
        var command = new SubscribeCustomerCommand(
            Guid.Empty,
            Guid.Empty,
            0,
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 1)); // End < Start

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.CustomerId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.ExternalPlanId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.MaxBeneficiaries));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(command.EndDate));
    }

    [Fact]
    public void SubscribeCustomerCommandValidator_WhenValid_ShouldPass()
    {
        var validator = new SubscribeCustomerCommandValidator();
        var command = new SubscribeCustomerCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            3,
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 12, 31));

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CancelSubscriptionCommandValidator_WhenEmptyId_ShouldFail()
    {
        var validator = new CancelSubscriptionCommandValidator();
        var result = validator.Validate(new CancelSubscriptionCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CancelSubscriptionCommand.SubscriptionId));
    }

    [Fact]
    public void UpdateMaxBeneficiariesCommandValidator_WhenZero_ShouldFail()
    {
        var validator = new UpdateMaxBeneficiariesCommandValidator();
        var result = validator.Validate(new UpdateMaxBeneficiariesCommand(Guid.NewGuid(), 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateMaxBeneficiariesCommand.NewMax));
    }
}
