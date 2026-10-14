using FluentValidation;
using FH.Customers.Domain.Common.ValueObjects;

namespace FH.Customers.Application.UseCases.Customers.Queries.GetCustomerByIdentification;

public class GetCustomerByIdentificationQueryValidator : AbstractValidator<GetCustomerByIdentificationQuery>
{
    public GetCustomerByIdentificationQueryValidator()
    {
        RuleFor(x => x.Type).NotEmpty().WithMessage("El tipo de identificación es obligatorio.");
        RuleFor(x => x.Type)
            .Must(DocumentId.IsValidType)
            .WithMessage($"El tipo de identificación no es válido. Valores permitidos: {DocumentId.AllowedTypes}.")
            .When(x => !string.IsNullOrWhiteSpace(x.Type));
        RuleFor(x => x.Number).NotEmpty().WithMessage("El número de identificación es obligatorio.");
    }
}
