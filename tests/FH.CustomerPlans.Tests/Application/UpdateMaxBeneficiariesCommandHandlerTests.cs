using Moq;
using Xunit;
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Application.CustomerPlans.Commands.UpdateMaxBeneficiaries;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.CustomerPlans.ValueObjects;

namespace FH.CustomerPlans.Tests.Application;

public class UpdateMaxBeneficiariesCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock = new();
    private readonly Mock<IAssignedBeneficiariesCounter> _counterMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateMaxBeneficiariesCommandHandler _handler;

    public UpdateMaxBeneficiariesCommandHandlerTests()
    {
        _handler = new UpdateMaxBeneficiariesCommandHandler(
            _repositoryMock.Object,
            _counterMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidAndAboveAssigned_ShouldUpdateSuccessfully()
    {
        // Arrange
        var subId = Guid.NewGuid();
        var sub = Subscription.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new MaxBeneficiaries(3),
            new DateOnly(2026, 1, 1));

        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        _counterMock.Setup(c => c.CountActiveBeneficiariesAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2); // RN-05: 2 beneficiarios asignados

        // Act: Ampliar a 5 cupos
        var result = await _handler.Handle(new UpdateMaxBeneficiariesCommand(subId, 5), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.MaxBeneficiaries);
        _repositoryMock.Verify(r => r.UpdateAsync(sub, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNewMaxLessThanAssignedBeneficiaries_ShouldReturnConflict()
    {
        // RN-05: El nuevo valor no puede ser menor a los beneficiarios asignados
        var subId = Guid.NewGuid();
        var sub = Subscription.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new MaxBeneficiaries(4),
            new DateOnly(2026, 1, 1));

        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        _counterMock.Setup(c => c.CountActiveBeneficiariesAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3); // 3 beneficiarios asignados

        // Act: Intentar reducir a 2
        var result = await _handler.Handle(new UpdateMaxBeneficiariesCommand(subId, 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("no puede ser menor a la cantidad de beneficiarios activos asignados", result.ErrorMessage);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionNotFound_ShouldReturnNotFound()
    {
        var subId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var result = await _handler.Handle(new UpdateMaxBeneficiariesCommand(subId, 5), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Handle_WhenSubscriptionSuspended_ShouldReturnUnprocessableEntity()
    {
        // RN-05: UpdateMaxBeneficiaries solo si está ACTIVE
        var subId = Guid.NewGuid();
        var sub = Subscription.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new MaxBeneficiaries(4),
            new DateOnly(2026, 1, 1));

        sub.Suspend();

        _repositoryMock.Setup(r => r.GetByIdAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        _counterMock.Setup(c => c.CountActiveBeneficiariesAsync(subId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.Handle(new UpdateMaxBeneficiariesCommand(subId, 6), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("activa", result.ErrorMessage);
    }
}
