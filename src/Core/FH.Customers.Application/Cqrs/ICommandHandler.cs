
using FH.Customers.Application.Common;
using FH.Customers.Application.Mediator;

namespace FH.Customers.Application.Cqrs;

/// <summary>
/// Manejador para un comando que no retorna valor.
/// </summary>
/// <typeparam name="TCommand">Tipo del comando.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Manejador para un comando que retorna un valor tipado.
/// </summary>
/// <typeparam name="TCommand">Tipo del comando.</typeparam>
/// <typeparam name="TResponse">Tipo de la respuesta en caso de éxito.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
