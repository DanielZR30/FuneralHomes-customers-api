
using FH.Customers.Domain.Common;

namespace FH.Customers.Application.Events;

/// <summary>
/// Reacciona a un evento de dominio (por ejemplo, registrar auditoría cuando se agrega un beneficiario).
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken);
}
