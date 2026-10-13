using FH.Customers.Application.Utilities.Mediator;
using FH.Customers.Application.Utilities.Results;

namespace FH.Customers.Application.UseCases.Common;

/// <summary>
/// Representa una consulta que obtiene datos con un resultado tipado.
/// </summary>
/// <typeparam name="TResponse">Tipo del dato consultado.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
