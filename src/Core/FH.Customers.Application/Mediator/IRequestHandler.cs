namespace FH.Customers.Application.Mediator;

/// <summary>
/// Caso de uso: maneja una petición y devuelve su respuesta.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
