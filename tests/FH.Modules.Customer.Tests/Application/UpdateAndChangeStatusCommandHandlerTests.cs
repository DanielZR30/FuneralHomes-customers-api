using FH.Modules.Customer.Application.Commands.ChangeCustomerStatus;
using FH.Modules.Customer.Application.Commands.UpdateCustomerDemographics;
using FH.Modules.Customer.Domain.Repositories;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Repositories;
using Moq;
using CustomerEntity = FH.Modules.Customer.Domain.Entities.Customer;

namespace FH.Modules.Customer.Tests.Application;

public class UpdateAndChangeStatusCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _customersMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateCustomerDemographicsCommandHandler _updateHandler;
    private readonly ChangeCustomerStatusCommandHandler _statusHandler;

    public UpdateAndChangeStatusCommandHandlerTests()
    {
        _updateHandler = new UpdateCustomerDemographicsCommandHandler(_customersMock.Object, _unitOfWorkMock.Object);
        _statusHandler = new ChangeCustomerStatusCommandHandler(_customersMock.Object, _unitOfWorkMock.Object);
    }

    private static CustomerEntity BuildCustomer(CustomerStatus status = CustomerStatus.Active)
    {
        var customer = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1010101010");

        if (status == CustomerStatus.Inactive)
        {
            customer.Deactivate();
        }
        else if (status == CustomerStatus.Suspended)
        {
            customer.Suspend();
        }

        return customer;
    }

    private void SetupExisting(CustomerEntity customer) =>
        _customersMock
            .Setup(c => c.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

    private void SetupMissing(Guid id) =>
        _customersMock
            .Setup(c => c.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

    // ---------- UpdateCustomerDemographics ----------

    [Fact]
    public async Task Update_WhenValid_ShouldReturnUpdatedCustomer()
    {
        var customer = BuildCustomer();
        SetupExisting(customer);

        var result = await _updateHandler.Handle(
            new UpdateCustomerDemographicsCommand(customer.Id, "Ana María", "nuevo@correo.com", "3009999999", "Av. 1"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Ana María", result.Value.Name);
        Assert.Equal("nuevo@correo.com", result.Value.Email);

        // RN-05: la identificación permanece intacta
        Assert.Equal("CC", result.Value.IdentificationType);
        Assert.Equal("1010101010", result.Value.IdentificationNumber);

        _customersMock.Verify(c => c.Update(customer), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenCustomerNotFound_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();
        SetupMissing(id);

        var result = await _updateHandler.Handle(
            new UpdateCustomerDemographicsCommand(id, "Ana"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenNameIsBlank_ShouldReturnBadRequest()
    {
        // RN-04
        var customer = BuildCustomer();
        SetupExisting(customer);

        var result = await _updateHandler.Handle(
            new UpdateCustomerDemographicsCommand(customer.Id, "   "),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(400, result.StatusCode);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- ChangeCustomerStatus ----------

    [Theory]
    [InlineData(CustomerStatus.Suspended)]
    [InlineData(CustomerStatus.Inactive)]
    public async Task ChangeStatus_FromActive_ShouldApplyTransition(CustomerStatus target)
    {
        var customer = BuildCustomer();
        SetupExisting(customer);

        var result = await _statusHandler.Handle(
            new ChangeCustomerStatusCommand(customer.Id, target),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(204, result.StatusCode);
        Assert.Equal(target, customer.Status);

        _customersMock.Verify(c => c.Update(customer), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeStatus_FromSuspendedToActive_ShouldReactivate()
    {
        var customer = BuildCustomer(CustomerStatus.Suspended);
        SetupExisting(customer);

        var result = await _statusHandler.Handle(
            new ChangeCustomerStatusCommand(customer.Id, CustomerStatus.Active),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CustomerStatus.Active, customer.Status);
    }

    [Fact]
    public async Task ChangeStatus_ToSameStatus_ShouldBeIdempotent()
    {
        // RN-06: pedir el estado actual no es un error y no escribe en la base de datos
        var customer = BuildCustomer(CustomerStatus.Suspended);
        SetupExisting(customer);

        var result = await _statusHandler.Handle(
            new ChangeCustomerStatusCommand(customer.Id, CustomerStatus.Suspended),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(204, result.StatusCode);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(CustomerStatus.Active)]
    [InlineData(CustomerStatus.Suspended)]
    public async Task ChangeStatus_FromInactive_ShouldReturnConflict(CustomerStatus target)
    {
        // RN-07: Inactive es terminal
        var customer = BuildCustomer(CustomerStatus.Inactive);
        SetupExisting(customer);

        var result = await _statusHandler.Handle(
            new ChangeCustomerStatusCommand(customer.Id, target),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(409, result.StatusCode);
        Assert.Contains("Inactivo", result.ErrorMessage);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangeStatus_WhenCustomerNotFound_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();
        SetupMissing(id);

        var result = await _statusHandler.Handle(
            new ChangeCustomerStatusCommand(id, CustomerStatus.Suspended),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
    }
}
