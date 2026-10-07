using FluentValidation;

namespace FH.Customers.Application.Beneficiaries.Commands.UpdateBeneficiary;

public class UpdateBeneficiaryCommandValidator : AbstractValidator<UpdateBeneficiaryCommand>
{
    public UpdateBeneficiaryCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty().WithMessage("El ID de la suscripción es obligatorio.");
        RuleFor(x => x.MemberId).NotEmpty().WithMessage("El ID del miembro es obligatorio.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre del beneficiario es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.LastName).MaximumLength(100).When(x => x.LastName is not null)
            .WithMessage("El apellido no puede superar 100 caracteres.");

        RuleFor(x => x.BirthDate)
            .Must(d => d != default).WithMessage("La fecha de nacimiento es obligatoria.")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La fecha de nacimiento no puede ser posterior a la fecha actual.");

        RuleFor(x => x.IdentificationNumber)
            .NotEmpty().WithMessage("Si indica el tipo de documento, debe indicar también el número.")
            .When(x => !string.IsNullOrWhiteSpace(x.IdentificationType));

        RuleFor(x => x.IdentificationType)
            .NotEmpty().WithMessage("Si indica el número de documento, debe indicar también el tipo.")
            .When(x => !string.IsNullOrWhiteSpace(x.IdentificationNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone).MaximumLength(30).When(x => x.Phone is not null)
            .WithMessage("El teléfono no puede superar 30 caracteres.");
    }
}
