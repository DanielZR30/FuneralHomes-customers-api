using FH.Modules.Customer.Application.DTOs;
using FH.Modules.Customer.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using CustomerEntity = FH.Shared.Domain.Entities.Customer;

namespace FH.Modules.Customer.Application.Commands.CreateCustomer;

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
        // RN-01: el nombre del titular es obligatorio.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CustomerResponse>.Failure(
                "El nombre del cliente es obligatorio.", "Validation", 400);
        }

        // RN-02: la identificación es obligatoria en sus dos partes.
        if (string.IsNullOrWhiteSpace(request.IdentificationType)
            || string.IsNullOrWhiteSpace(request.IdentificationNumber))
        {
            return Result<CustomerResponse>.Failure(
                "El tipo y el número de identificación son obligatorios.", "Validation", 400);
        }

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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // RN-03 (defensa): el índice IX_customers_identification_number es UNIQUE
            // sobre el número únicamente, más estricto que la regla de negocio. Si dos
            // tipos de documento distintos comparten número, la base de datos rechaza
            // el INSERT y se traduce a 409 en lugar de exponer un 500.
            return Result<CustomerResponse>.Conflict(
                $"Ya existe un cliente registrado con el número de identificación {identificationNumber}.");
        }

        return Result<CustomerResponse>.Success(CustomerResponse.FromEntity(customer), 201);
    }
}
