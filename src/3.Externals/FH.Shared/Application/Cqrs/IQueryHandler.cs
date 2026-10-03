using FH.Shared.Application.Common;
using MediatR;

namespace FH.Shared.Application.Cqrs;

/// <summary>
/// Manejador para una consulta en el patrón CQRS.
/// </summary>
/// <typeparam name="TQuery">Tipo de la consulta.</typeparam>
/// <typeparam name="TResponse">Tipo del dato obtenido.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
