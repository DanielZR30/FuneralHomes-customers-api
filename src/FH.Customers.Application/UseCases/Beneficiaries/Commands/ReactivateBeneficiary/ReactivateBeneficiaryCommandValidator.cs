using FluentValidation;

namespace FH.Customers.Application.UseCases.Beneficiaries.Commands.ReactivateBeneficiary;

public class ReactivateBeneficiaryCommandValidator : AbstractValidator<ReactivateBeneficiaryCommand>
{
    public ReactivateBeneficiaryCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty().WithMessage("El ID de la suscripción es obligatorio.");
        RuleFor(x => x.MemberId).NotEmpty().WithMessage("El ID del miembro es obligatorio.");
    }
}
