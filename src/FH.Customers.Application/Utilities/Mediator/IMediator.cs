namespace FH.Customers.Application.Utilities.Mediator;

/// <summary>
/// Punto único por el que la API envía peticiones a los casos de uso.
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
