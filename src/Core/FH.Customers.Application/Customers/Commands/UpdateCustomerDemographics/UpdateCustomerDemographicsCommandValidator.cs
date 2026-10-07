using FluentValidation;

namespace FH.Customers.Application.Customers.Commands.UpdateCustomerDemographics;

public class UpdateCustomerDemographicsCommandValidator : AbstractValidator<UpdateCustomerDemographicsCommand>
{
    public UpdateCustomerDemographicsCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El ID del cliente es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
            .MaximumLength(255).WithMessage("El nombre no puede superar 255 caracteres.");

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
