using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Application.Commands.SubscribeCustomer;
using FH.Modules.CustomerPlans.Domain;
using FH.Modules.CustomerPlans.Domain.ValueObjects;
using Moq;
using Xunit;

namespace FH.Modules.CustomerPlans.Tests.Application;

public class SubscribeCustomerCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock = new();
    private readonly Mock<ICustomerExistenceChecker> _customerCheckerMock = new();
    private readonly Mock<IFinancialsPlanGateway> _planGatewayMock = new();
    private readonly Mock<ICustomerPlansUnitOfWork> _unitOfWorkMock = new();

    private readonly SubscribeCustomerCommandHandler _handler;

    public SubscribeCustomerCommandHandlerTests()
    {
        _handler = new SubscribeCustomerCommandHandler(
            _repositoryMock.Object,
            _customerCheckerMock.Object,
            _planGatewayMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateSubscriptionAndReturnSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new SubscribeCustomerCommand(customerId, planId, 4, new DateOnly(2026, 10, 1));

        _customerCheckerMock.Setup(c => c.ExistsActiveAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // RN-06

        _planGatewayMock.Setup(p => p.PlanExistsAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // RN-08

        _repositoryMock.Setup(r => r.GetActiveByCustomerAndPlanAsync(customerId, planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription>()); // RN-07: no overlaps

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal(customerId, result.Value.CustomerId);
        Assert.Equal(planId, result.Value.ExternalPlanId);
        Assert.Equal(4, result.Value.MaxBeneficiaries);
        Assert.Equal("ACTIVE", result.Value.Status);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExistOrIsInactive_ShouldReturnNotFound()
    {
        // RN-06
        var customerId = Guid.NewGuid();
        var command = new SubscribeCustomerCommand(customerId, Guid.NewGuid(), 2, new DateOnly(2026, 10, 1));

        _customerCheckerMock.Setup(c => c.ExistsActiveAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("no existe o no se encuentra activo", result.ErrorMessage);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenFinancialPlanDoesNotExist_ShouldReturnUnprocessableEntity()
    {
        // RN-08
        var customerId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new SubscribeCustomerCommand(customerId, planId, 2, new DateOnly(2026, 10, 1));

        _customerCheckerMock.Setup(c => c.ExistsActiveAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _planGatewayMock.Setup(p => p.PlanExistsAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(422, result.StatusCode);
        Assert.Contains("plan financiero especificado no existe", result.ErrorMessage);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOverlappingActiveSubscriptionExists_ShouldReturnConflict()
    {
        // RN-07: Períodos solapados para mismo cliente y mismo external_plan_id
        var customerId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new SubscribeCustomerCommand(
            customerId,
            planId,
            3,
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 12, 31));

        _customerCheckerMock.Setup(c => c.ExistsActiveAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _planGatewayMock.Setup(p => p.PlanExistsAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var existingSub = Subscription.Create(
            customerId,
            planId,
            new MaxBeneficiaries(2),
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 10, 15)); // Solapa con 2026-10-01 a 2026-12-31

        _repositoryMock.Setup(r => r.GetActiveByCustomerAndPlanAsync(customerId, planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription> { existingSub });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("solapa", result.ErrorMessage);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNonOverlappingActiveSubscriptionExists_ShouldSucceed()
    {
        // RN-07: Mismo plan pero períodos distintos (sin solapamiento)
        var customerId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new SubscribeCustomerCommand(
            customerId,
            planId,
            3,
            new DateOnly(2026, 11, 1),
            new DateOnly(2026, 12, 31));

        _customerCheckerMock.Setup(c => c.ExistsActiveAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _planGatewayMock.Setup(p => p.PlanExistsAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var existingSub = Subscription.Create(
            customerId,
            planId,
            new MaxBeneficiaries(2),
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 10, 15)); // Termina antes de 2026-11-01

        _repositoryMock.Setup(r => r.GetActiveByCustomerAndPlanAsync(customerId, planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Subscription> { existingSub });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.StatusCode);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
