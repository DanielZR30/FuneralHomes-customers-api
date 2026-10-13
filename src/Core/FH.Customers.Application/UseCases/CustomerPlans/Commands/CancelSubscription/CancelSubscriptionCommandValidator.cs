using FluentValidation;

namespace FH.Customers.Application.UseCases.CustomerPlans.Commands.CancelSubscription;

public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty()
            .WithMessage("El ID de la suscripción es obligatorio.");
    }
}
