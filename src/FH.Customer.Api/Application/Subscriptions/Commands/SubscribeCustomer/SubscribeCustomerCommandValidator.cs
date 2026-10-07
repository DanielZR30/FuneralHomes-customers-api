using FluentValidation;

namespace FH.Modules.CustomerPlans.Application.Commands.SubscribeCustomer;

public class SubscribeCustomerCommandValidator : AbstractValidator<SubscribeCustomerCommand>
{
    public SubscribeCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("El ID del cliente es obligatorio.");

        RuleFor(x => x.ExternalPlanId)
            .NotEmpty()
            .WithMessage("El ID del plan externo es obligatorio.");

        RuleFor(x => x.MaxBeneficiaries)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El cupo máximo de beneficiarios debe ser mayor o igual a 1.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("La fecha de finalización no puede ser anterior a la fecha de inicio.");
    }
}
