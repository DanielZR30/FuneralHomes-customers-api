using FH.Modules.CustomerPlans.Domain;
using FH.Modules.CustomerPlans.Domain.Events;
using FH.Modules.CustomerPlans.Domain.ValueObjects;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Exceptions;
using Xunit;

namespace FH.Modules.CustomerPlans.Tests.Domain;

public class SubscriptionTests
{
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _externalPlanId = Guid.NewGuid();
    private readonly DateOnly _startDate = new(2026, 10, 1);

    [Fact]
    public void CreateSubscription_WithValidData_ShouldSetStateToActiveAndRaiseEvent()
    {
        // Act
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(3), _startDate);

        // Assert
        Assert.NotEqual(Guid.Empty, sub.Id);
        Assert.Equal(_customerId, sub.CustomerId);
        Assert.Equal(_externalPlanId, sub.ExternalPlanId);
        Assert.Equal(3, sub.MaxBeneficiaries.Value);
        Assert.Equal(_startDate, sub.Period.StartDate);
        Assert.Null(sub.Period.EndDate);
        Assert.Equal(SubscriptionStatus.Active, sub.Status); // RN-03

        var createdEvent = Assert.Single(sub.DomainEvents);
        var subCreated = Assert.IsType<SubscriptionCreated>(createdEvent);
        Assert.Equal(sub.Id, subCreated.SubscriptionId);
        Assert.Equal(_customerId, subCreated.CustomerId);
        Assert.Equal(3, subCreated.MaxBeneficiaries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void MaxBeneficiaries_LessThanOne_ShouldThrowBusinessRuleException(int value)
    {
        // RN-01
        var ex = Assert.Throws<BusinessRuleException>(() => new MaxBeneficiaries(value));
        Assert.Contains("mayor o igual a 1", ex.Message);
    }

    [Fact]
    public void SubscriptionPeriod_WithEndDateEarlierThanStartDate_ShouldThrowBusinessRuleException()
    {
        // RN-02
        var ex = Assert.Throws<BusinessRuleException>(() =>
            new SubscriptionPeriod(new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 5)));
        Assert.Contains("anterior a la fecha de inicio", ex.Message);
    }

    [Fact]
    public void CancelSubscription_WhenActiveAndTodayAfterStartDate_ShouldSetEndDateToToday()
    {
        // Arrange
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), new DateOnly(2026, 10, 1));
        var today = new DateOnly(2026, 10, 15);

        // Act
        sub.CancelSubscription(today);

        // Assert
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
        Assert.Equal(today, sub.Period.EndDate);

        var cancelEvent = sub.DomainEvents.OfType<SubscriptionCancelled>().Single();
        Assert.Equal(sub.Id, cancelEvent.SubscriptionId);
        Assert.Equal(today, cancelEvent.CancelledDate);
    }

    [Fact]
    public void CancelSubscription_WhenActiveAndTodayBeforeStartDate_ShouldSetEndDateToStartDate()
    {
        // RN-04: EndDate = max(hoy UTC, StartDate)
        var futureStart = new DateOnly(2026, 11, 1);
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), futureStart);
        var today = new DateOnly(2026, 10, 15);

        // Act
        sub.CancelSubscription(today);

        // Assert
        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
        Assert.Equal(futureStart, sub.Period.EndDate);
    }

    [Fact]
    public void CancelSubscription_WhenSuspended_ShouldSucceed()
    {
        // RN-04: Cancel allowed from SUSPENDED
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);
        sub.Suspend();

        var today = new DateOnly(2026, 10, 5);
        sub.CancelSubscription(today);

        Assert.Equal(SubscriptionStatus.Cancelled, sub.Status);
    }

    [Fact]
    public void CancelSubscription_WhenAlreadyCancelled_ShouldThrowBusinessRuleException()
    {
        // RN-04: Cancel from CANCELLED throws
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);
        sub.CancelSubscription(new DateOnly(2026, 10, 5));

        var ex = Assert.Throws<BusinessRuleException>(() => sub.CancelSubscription(new DateOnly(2026, 10, 6)));
        Assert.Contains("CANCELLED", ex.Message);
    }

    [Fact]
    public void CancelSubscription_WhenExpired_ShouldThrowBusinessRuleException()
    {
        // RN-04: Cancel from EXPIRED throws
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);
        sub.Expire(new DateOnly(2026, 10, 5));

        var ex = Assert.Throws<BusinessRuleException>(() => sub.CancelSubscription(new DateOnly(2026, 10, 6)));
        Assert.Contains("EXPIRED", ex.Message);
    }

    [Fact]
    public void UpdateMaxBeneficiaries_WhenActive_ShouldUpdateAndRaiseEvent()
    {
        // RN-05
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);

        sub.UpdateMaxBeneficiaries(new MaxBeneficiaries(5));

        Assert.Equal(5, sub.MaxBeneficiaries.Value);
        var updatedEvent = sub.DomainEvents.OfType<MaxBeneficiariesUpdated>().Single();
        Assert.Equal(2, updatedEvent.PreviousMax);
        Assert.Equal(5, updatedEvent.NewMax);
    }

    [Fact]
    public void UpdateMaxBeneficiaries_WhenSuspended_ShouldThrowBusinessRuleException()
    {
        // RN-05: Update only when ACTIVE
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);
        sub.Suspend();

        var ex = Assert.Throws<BusinessRuleException>(() => sub.UpdateMaxBeneficiaries(new MaxBeneficiaries(5)));
        Assert.Contains("activa", ex.Message);
    }

    [Fact]
    public void SuspendAndReactivate_ShouldTransitionCorrectly()
    {
        // State transitions
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);

        sub.Suspend();
        Assert.Equal(SubscriptionStatus.Suspended, sub.Status);

        sub.Reactivate();
        Assert.Equal(SubscriptionStatus.Active, sub.Status);
    }

    [Fact]
    public void Reactivate_WhenActive_ShouldThrowBusinessRuleException()
    {
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);

        var ex = Assert.Throws<BusinessRuleException>(() => sub.Reactivate());
        Assert.Contains("suspendida", ex.Message);
    }

    [Fact]
    public void Expire_WhenActive_ShouldSetStatusToExpired()
    {
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);
        var today = new DateOnly(2026, 10, 20);

        sub.Expire(today);

        Assert.Equal(SubscriptionStatus.Expired, sub.Status);
        Assert.Equal(today, sub.Period.EndDate);
    }

    [Fact]
    public void Expire_WhenAlreadyExpired_ShouldThrowBusinessRuleException()
    {
        var sub = Subscription.Create(_customerId, _externalPlanId, new MaxBeneficiaries(2), _startDate);
        sub.Expire(new DateOnly(2026, 10, 20));

        var ex = Assert.Throws<BusinessRuleException>(() => sub.Expire(new DateOnly(2026, 10, 21)));
        Assert.Contains("EXPIRED", ex.Message);
    }
}
