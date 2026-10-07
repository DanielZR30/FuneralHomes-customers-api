using FH.Modules.Customer.Application.Abstractions;
using FH.Modules.Customer.Application.DTOs;
using FH.Modules.Customer.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;

namespace FH.Modules.Customer.Application.Commands.UpdateCustomerDemographics;

public class UpdateCustomerDemographicsCommandHandler
    : ICommandHandler<UpdateCustomerDemographicsCommand, CustomerResponse>
{
    private readonly ICustomerRepository _customers;
    private readonly ICustomerUnitOfWork _unitOfWork;

    public UpdateCustomerDemographicsCommandHandler(
        ICustomerRepository customers,
        ICustomerUnitOfWork unitOfWork)
    {
        _customers = customers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerResponse>> Handle(
        UpdateCustomerDemographicsCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result<CustomerResponse>.NotFound($"El cliente {request.Id} no existe.");
        }

        // RN-04: el nombre no puede quedar vacío.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CustomerResponse>.Failure(
                "El nombre del cliente es obligatorio.", "Validation", 400);
        }

        // RN-05: el documento de identificación es INMUTABLE. El comando no lo expone,
        // por lo que un PUT nunca puede alterarlo; solo puede cambiarse la demografía
        // y los datos de contacto.
        try
        {
            customer.UpdateDemographics(request.Name, request.Email, request.Phone, request.Address);
        }
        catch (ArgumentException ex)
        {
            return Result<CustomerResponse>.Failure(ex.Message, "BusinessRule", 400);
        }

        _customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CustomerResponse>.Success(CustomerResponse.FromEntity(customer));
    }
}
