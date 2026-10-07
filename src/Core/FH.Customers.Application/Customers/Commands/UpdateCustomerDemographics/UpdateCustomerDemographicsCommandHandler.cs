
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.Customers.DTOs;
using FH.Customers.Domain.Customers.Repositories;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Application.Customers.Commands.UpdateCustomerDemographics;

public class UpdateCustomerDemographicsCommandHandler
    : ICommandHandler<UpdateCustomerDemographicsCommand, CustomerResponse>
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerDemographicsCommandHandler(
        ICustomerRepository customers,
        IUnitOfWork unitOfWork)
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

        // RN-04 (nombre no vacío) se valida en UpdateCustomerDemographicsCommandValidator.
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
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<CustomerResponse>.Success(CustomerResponse.FromEntity(customer));
    }
}
