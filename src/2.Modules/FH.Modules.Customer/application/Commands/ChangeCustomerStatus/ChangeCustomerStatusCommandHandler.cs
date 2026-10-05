using FH.Modules.Customer.Domain.Repositories;
using FH.Shared.Application.Common;
using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Repositories;

namespace FH.Modules.Customer.Application.Commands.ChangeCustomerStatus;

public class ChangeCustomerStatusCommandHandler : ICommandHandler<ChangeCustomerStatusCommand>
{
    private readonly ICustomerRepository _customers;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeCustomerStatusCommandHandler(
        ICustomerRepository customers,
        IUnitOfWork unitOfWork)
    {
        _customers = customers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ChangeCustomerStatusCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.NotFound($"El cliente {request.Id} no existe.");
        }

        // RN-06: la transición al estado que ya se tiene es idempotente, no es un error.
        if (customer.Status == request.Status)
        {
            return Result.Success(204);
        }

        // RN-07: Inactive es un estado terminal. Un titular desactivado queda fuera
        // del ciclo operativo y no admite reapertura, para no alterar la trazabilidad
        // histórica de sus planes y Beneficiarios.
        if (customer.Status == CustomerStatus.Inactive)
        {
            return Result.Conflict(
                $"El cliente está Inactivo y no admite más cambios de estado (solicitado: {request.Status}).");
        }

        switch (request.Status)
        {
            case CustomerStatus.Active:
                customer.Activate();
                break;

            case CustomerStatus.Inactive:
                customer.Deactivate();
                break;

            case CustomerStatus.Suspended:
                customer.Suspend();
                break;
        }

        _customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(204);
    }
}
