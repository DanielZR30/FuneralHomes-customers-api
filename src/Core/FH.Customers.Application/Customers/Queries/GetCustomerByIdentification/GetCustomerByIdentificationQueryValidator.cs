using FluentValidation;

namespace FH.Customers.Application.Customers.Queries.GetCustomerByIdentification;

public class GetCustomerByIdentificationQueryValidator : AbstractValidator<GetCustomerByIdentificationQuery>
{
    public GetCustomerByIdentificationQueryValidator()
    {
        RuleFor(x => x.Type).NotEmpty().WithMessage("El tipo de identificación es obligatorio.");
        RuleFor(x => x.Number).NotEmpty().WithMessage("El número de identificación es obligatorio.");
    }
}
