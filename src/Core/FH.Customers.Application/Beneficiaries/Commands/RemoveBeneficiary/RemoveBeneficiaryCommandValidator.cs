using FluentValidation;

namespace FH.Customers.Application.Beneficiaries.Commands.RemoveBeneficiary;

public class RemoveBeneficiaryCommandValidator : AbstractValidator<RemoveBeneficiaryCommand>
{
    public RemoveBeneficiaryCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty().WithMessage("El ID de la suscripción es obligatorio.");
        RuleFor(x => x.MemberId).NotEmpty().WithMessage("El ID del miembro es obligatorio.");
    }
}
