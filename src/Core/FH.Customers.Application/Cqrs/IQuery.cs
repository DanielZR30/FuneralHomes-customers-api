
using FH.Customers.Application.Common;
using FH.Customers.Application.Mediator;

namespace FH.Customers.Application.Cqrs;

/// <summary>
/// Representa una consulta que obtiene datos con un resultado tipado.
/// </summary>
/// <typeparam name="TResponse">Tipo del dato consultado.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
