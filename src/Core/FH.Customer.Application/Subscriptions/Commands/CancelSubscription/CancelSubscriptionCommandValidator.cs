using FluentValidation;

namespace FH.Modules.CustomerPlans.Application.Commands.CancelSubscription;

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty()
            .WithMessage("El ID de la suscripción es obligatorio.");
    }
}
