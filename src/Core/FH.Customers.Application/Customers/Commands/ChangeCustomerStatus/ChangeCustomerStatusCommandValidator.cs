using FluentValidation;

namespace FH.Customers.Application.Customers.Commands.ChangeCustomerStatus;

public class ChangeCustomerStatusCommandValidator : AbstractValidator<ChangeCustomerStatusCommand>
{
    public ChangeCustomerStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El ID del cliente es obligatorio.");
        RuleFor(x => x.Status).IsInEnum().WithMessage("El estado indicado no es válido.");
    }
}
