using Moq;
using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using FH.Customers.Application.Customers.Queries.GetCustomerById;
using FH.Customers.Application.Customers.Queries.GetCustomerByIdentification;
using FH.Customers.Application.Customers.Queries.GetCustomers;
using FH.Customers.Domain.Customers.Repositories;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Tests.Application;

public class QueriesHandlerTests
{
    private readonly Mock<ICustomerRepository> _customersMock = new();
    private readonly GetCustomerByIdQueryHandler _byIdHandler;
    private readonly GetCustomerByIdentificationQueryHandler _byIdentificationHandler;
    private readonly GetCustomersQueryHandler _listHandler;

    public QueriesHandlerTests()
    {
        _byIdHandler = new GetCustomerByIdQueryHandler(_customersMock.Object);
        _byIdentificationHandler = new GetCustomerByIdentificationQueryHandler(_customersMock.Object);
        _listHandler = new GetCustomersQueryHandler(_customersMock.Object);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnCustomer()
    {
        var customer = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1010101010");

        _customersMock
            .Setup(c => c.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _byIdHandler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customer.Id, result.Value.Id);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        var id = Guid.NewGuid();

        _customersMock
            .Setup(c => c.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        var result = await _byIdHandler.Handle(new GetCustomerByIdQuery(id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdentification_WhenExists_ShouldReturnCustomer()
    {
        var customer = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1010101010");

        _customersMock
            .Setup(c => c.GetByIdentificationAsync("CC", "1010101010", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _byIdentificationHandler.Handle(
            new GetCustomerByIdentificationQuery("CC", "1010101010"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(customer.Id, result.Value.Id);
    }

    [Fact]
    public async Task GetByIdentification_ShouldDelegateToRepository()
    {
        // La normalización (Trim + ToUpperInvariant) es responsabilidad de
        // CustomerRepository, en el límite de datos: el handler entrega tal cual
        // lo recibido para que ningún llamador pueda omitirla.
        _customersMock
            .Setup(c => c.GetByIdentificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        await _byIdentificationHandler.Handle(
            new GetCustomerByIdentificationQuery("  cc ", " 1010101010 "),
            CancellationToken.None);

        _customersMock.Verify(
            c => c.GetByIdentificationAsync("  cc ", " 1010101010 ", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdentification_WhenNotFound_ShouldReturnNotFound()
    {
        _customersMock
            .Setup(c => c.GetByIdentificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        var result = await _byIdentificationHandler.Handle(
            new GetCustomerByIdentificationQuery("CC", "999"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_WithoutFilter_ShouldReturnAllOrderedByName()
    {
        var zulu = CustomerEntity.Create(CustomerType.Individual, "Zulma", "CC", "3");
        var ana = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1");

        _customersMock
            .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerEntity> { zulu, ana });

        var result = await _listHandler.Handle(new GetCustomersQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "Ana", "Zulma" }, result.Value.Select(c => c.Name).ToArray());
    }

    [Fact]
    public async Task GetCustomers_WithStatusFilter_ShouldUseFindAsync()
    {
        var active = CustomerEntity.Create(CustomerType.Individual, "Ana", "CC", "1");
        var suspended = CustomerEntity.Create(CustomerType.Individual, "Luis", "CC", "2");
        suspended.Suspend();

        _customersMock
            .Setup(c => c.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CustomerEntity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerEntity> { active });

        var result = await _listHandler.Handle(
            new GetCustomersQuery(CustomerStatus.Active),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Ana", result.Value[0].Name);
    }
}
