using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.Customers.DTOs;
using FH.Customers.Domain.Customers.Repositories;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, CustomerResponse>
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ICustomerRepository customers,
        IUnitOfWork unitOfWork)
    {
        _customers = customers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerResponse>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        // RN-01 y RN-02 (nombre e identificación obligatorios) se validan en CreateCustomerCommandValidator.
        // RN-03: la pareja (tipo, número) de identificación es única en todo el sistema.
        var identificationType = request.IdentificationType.Trim().ToUpperInvariant();
        var identificationNumber = request.IdentificationNumber.Trim().ToUpperInvariant();

        var alreadyRegistered = await _customers.ExistsByIdentificationAsync(
            identificationType,
            identificationNumber,
            cancellationToken);

        if (alreadyRegistered)
        {
            return Result<CustomerResponse>.Conflict(
                $"Ya existe un cliente registrado con la identificación {identificationType} {identificationNumber}.");
        }

        CustomerEntity customer;

        try
        {
            customer = CustomerEntity.Create(
                request.CustomerType,
                request.Name,
                identificationType,
                identificationNumber,
                request.Email,
                request.Phone,
                request.Address);
        }
        catch (ArgumentException ex)
        {
            return Result<CustomerResponse>.Failure(ex.Message, "BusinessRule", 400);
        }

        await _customers.AddAsync(customer, cancellationToken);

        try
        {
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DataConflictException)
        {
            // RN-03 (defensa): si dos solicitudes crean el mismo documento a la vez, el índice único
            // (tipo + número) rechaza la segunda y se responde 409 en lugar de un 500.
            return Result<CustomerResponse>.Conflict(
                $"Ya existe un cliente registrado con el número de identificación {identificationNumber}.");
        }

        return Result<CustomerResponse>.Success(CustomerResponse.FromEntity(customer), 201);
    }
}
