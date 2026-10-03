using FH.Shared.Application.Common;
using MediatR;

namespace FH.Shared.Application.Cqrs;

/// <summary>
/// Representa una consulta que obtiene datos con un resultado tipado.
/// </summary>
/// <typeparam name="TResponse">Tipo del dato consultado.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
