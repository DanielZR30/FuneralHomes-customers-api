using FluentValidation;

namespace FH.Customers.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerType).IsInEnum().WithMessage("El tipo de cliente no es válido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
            .MaximumLength(255).WithMessage("El nombre no puede superar 255 caracteres.");

        RuleFor(x => x.IdentificationType)
            .NotEmpty().WithMessage("El tipo de identificación es obligatorio.")
            .MaximumLength(20).WithMessage("El tipo de identificación no puede superar 20 caracteres.");

        RuleFor(x => x.IdentificationNumber)
            .NotEmpty().WithMessage("El número de identificación es obligatorio.")
            .MaximumLength(50).WithMessage("El número de identificación no puede superar 50 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone).MaximumLength(30).When(x => x.Phone is not null)
            .WithMessage("El teléfono no puede superar 30 caracteres.");

        RuleFor(x => x.Address).MaximumLength(255).When(x => x.Address is not null)
            .WithMessage("La dirección no puede superar 255 caracteres.");
    }
}
