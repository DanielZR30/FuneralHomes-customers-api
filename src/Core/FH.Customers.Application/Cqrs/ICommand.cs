
using FH.Customers.Application.Common;
using FH.Customers.Application.Mediator;

namespace FH.Customers.Application.Cqrs;

/// <summary>
/// Representa un comando que produce un resultado de operación sin valor de retorno.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Representa un comando que produce un resultado con un valor de retorno tipado.
/// </summary>
/// <typeparam name="TResponse">Tipo del valor retornado en caso de éxito.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
