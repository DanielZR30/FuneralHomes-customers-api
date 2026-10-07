
using FH.Customers.Application.Common;
using FH.Customers.Application.Mediator;

namespace FH.Customers.Application.Cqrs;

/// <summary>
/// Manejador para una consulta en el patrón CQRS.
/// </summary>
/// <typeparam name="TQuery">Tipo de la consulta.</typeparam>
/// <typeparam name="TResponse">Tipo del dato obtenido.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
