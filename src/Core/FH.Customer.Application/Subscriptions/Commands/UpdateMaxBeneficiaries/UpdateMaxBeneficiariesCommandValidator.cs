using FluentValidation;

namespace FH.Modules.CustomerPlans.Application.Commands.UpdateMaxBeneficiaries;

public class UpdateMaxBeneficiariesCommandValidator : AbstractValidator<UpdateMaxBeneficiariesCommand>
{
    public UpdateMaxBeneficiariesCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty()
            .WithMessage("El ID de la suscripción es obligatorio.");

        RuleFor(x => x.NewMax)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El cupo máximo de beneficiarios debe ser mayor o igual a 1.");
    }
}
