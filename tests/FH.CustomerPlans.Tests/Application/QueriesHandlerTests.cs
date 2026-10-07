using Moq;
using Xunit;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Application.CustomerPlans.Queries.GetCustomerSubscriptions;
using FH.Customers.Application.CustomerPlans.Queries.GetSubscriptionById;
using FH.Customers.Application.CustomerPlans.Queries.GetSubscriptionCapacity;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.CustomerPlans.ValueObjects;

namespace FH.CustomerPlans.Tests.Application;

public class QueriesHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock = new();
    private readonly Mock<IAssignedBeneficiariesCounter> _counterMock = new();

    [Fact]
    public async Task GetCustomerSubscriptions_WhenCustomerHasSubscriptions_ShouldReturnList()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var sub1 = Subscription.Create(customerId, Guid.NewGuid(), new MaxBeneficiaries(2), new DateOnly(2026, 1, 1));
        var sub2 = Subscription.Create(customerId, Guid.NewGuid(), new MaxBeneficiaries(4), new DateOnly(2026, 6, 1));

        _repositoryMock.Setup(r => r.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription> { sub1, sub2 });

        var handler = new GetCustomerSubscriptionsQueryHandler(_repositoryMock.Object);

        // Act
        var result = await handler.Handle(new GetCustomerSubscriptionsQuery(customerId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetCustomerSubscriptions_WhenCustomerHasNoSubscriptions_ShouldReturnEmptyList()
    {
        var customerId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription>());

        var handler = new GetCustomerSubscriptionsQueryHandler(_repositoryMock.Object);

        var result = await handler.Handle(new GetCustomerSubscriptionsQuery(customerId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetSubscriptionById_WhenExists_ShouldReturnDto()
    {
        var subId = Guid.NewGuid();
        var sub = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), new MaxBeneficiaries(3), new DateOnly(2026, 1, 1));

        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        var handler = new GetSubscriptionByIdQueryHandler(_repositoryMock.Object);

        var result = await handler.Handle(new GetSubscriptionByIdQuery(subId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.MaxBeneficiaries);
    }

    [Fact]
    public async Task GetSubscriptionById_WhenDoesNotExist_ShouldReturnNotFound()
    {
        var subId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var handler = new GetSubscriptionByIdQueryHandler(_repositoryMock.Object);

        var result = await handler.Handle(new GetSubscriptionByIdQuery(subId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetSubscriptionCapacity_ShouldComputeAvailableSpotsCorrectly()
    {
        var subId = Guid.NewGuid();
        var sub = Subscription.Create(Guid.NewGuid(), Guid.NewGuid(), new MaxBeneficiaries(5), new DateOnly(2026, 1, 1));

        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        _counterMock.Setup(c => c.CountActiveBeneficiariesAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2); // 2 ocupados

        var handler = new GetSubscriptionCapacityQueryHandler(_repositoryMock.Object, _counterMock.Object);

        var result = await handler.Handle(new GetSubscriptionCapacityQuery(subId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.MaxBeneficiaries);
        Assert.Equal(2, result.Value.AssignedBeneficiaries);
        Assert.Equal(3, result.Value.AvailableCapacity); // 5 - 2 = 3
        Assert.True(result.Value.CanAcceptBeneficiary);
    }
}
