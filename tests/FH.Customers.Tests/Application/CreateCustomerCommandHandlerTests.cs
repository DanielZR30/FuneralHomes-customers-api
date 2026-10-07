using Moq;
using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Customers.Commands.CreateCustomer;
using FH.Customers.Application.Customers.DTOs;
using FH.Customers.Domain.Customers.Repositories;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Tests.Application;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customersMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _handler = new CreateCustomerCommandHandler(_customersMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateActiveCustomerAndReturn201()
    {
        // Arrange
        _customersMock
            .Setup(c => c.ExistsByIdentificationAsync("CC", "1010101010", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customersMock
            .Setup(c => c.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(
            new CreateCustomerCommand(
                CustomerType.Individual,
                "Ana María Gómez",
                "cc",
                " 1010101010 ",
                "ana@correo.com",
                "3001234567",
                "Calle 1"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.StatusCode);
        Assert.Equal(CustomerStatus.Active.ToString(), result.Value.Status);
        Assert.Equal("CC", result.Value.IdentificationType);
        Assert.Equal("1010101010", result.Value.IdentificationNumber);

        _customersMock.Verify(
            c => c.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenIdentificationAlreadyExists_ShouldReturnConflict()
    {
        // RN-03: la pareja (tipo, número) es única en todo el sistema
        _customersMock
            .Setup(c => c.ExistsByIdentificationAsync("CC", "1010101010", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new CreateCustomerCommand(CustomerType.Individual, "Ana", "CC", "1010101010"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("Ya existe un cliente registrado", result.ErrorMessage);

        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDatabaseRejectsDuplicateNumber_ShouldReturnConflict()
    {
        // RN-03 (defensa): IX_customers_identification_number es UNIQUE sobre el número
        // solo, más estricto que la regla de negocio.
        _customersMock
            .Setup(c => c.ExistsByIdentificationAsync("CC", "123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customersMock
            .Setup(c => c.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DataConflictException("duplicate key"));

        var result = await _handler.Handle(
            new CreateCustomerCommand(CustomerType.Corporate, "Funeraria Norte", "NIT", "123"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(409, result.StatusCode);
    }
}
