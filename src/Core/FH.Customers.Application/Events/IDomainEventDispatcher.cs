
using FH.Customers.Domain.Common;

namespace FH.Customers.Application.Events;

/// <summary>
/// Entrega un evento de dominio a todos sus handlers registrados.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
