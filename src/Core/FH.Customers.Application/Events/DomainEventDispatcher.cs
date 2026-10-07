using Microsoft.Extensions.DependencyInjection;
using FH.Customers.Domain.Common;

namespace FH.Customers.Application.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handle = handlerType.GetMethod("Handle")!;

        foreach (var handler in _serviceProvider.GetServices(handlerType))
        {
            await (Task)handle.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
        }
    }
}
