using Moq;
using Xunit;
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Application.CustomerPlans.Commands.CancelSubscription;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.CustomerPlans.ValueObjects;
using FH.Customers.Domain.Enums;

namespace FH.CustomerPlans.Tests.Application;

public class CancelSubscriptionCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CancelSubscriptionCommandHandler _handler;

    public CancelSubscriptionCommandHandlerTests()
    {
        _handler = new CancelSubscriptionCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionExistsAndActive_ShouldCancelAndReturnSuccess()
    {
        // Arrange
        var subId = Guid.NewGuid();
        var sub = Subscription.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new MaxBeneficiaries(2),
            new DateOnly(2026, 1, 1));

        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        // Act
        var result = await _handler.Handle(new CancelSubscriptionCommand(subId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("CANCELLED", result.Value.Status);
        Assert.NotNull(result.Value.EndDate);

        _repositoryMock.Verify(r => r.UpdateAsync(sub, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionNotFound_ShouldReturnNotFound()
    {
        var subId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var result = await _handler.Handle(new CancelSubscriptionCommand(subId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Handle_WhenAlreadyCancelled_ShouldReturnUnprocessableEntity()
    {
        // RN-04: Cancelar desde CANCELLED lanza error de negocio
        var subId = Guid.NewGuid();
        var sub = Subscription.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new MaxBeneficiaries(2),
            new DateOnly(2026, 1, 1));

        sub.CancelSubscription(new DateOnly(2026, 5, 1));

        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        // Act
        var result = await _handler.Handle(new CancelSubscriptionCommand(subId), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("CANCELLED", result.ErrorMessage);
    }
}
