using FH.Messaging.Application;
using Microsoft.Extensions.Logging;

namespace FH.Customer.Infrastructure.Messaging;

public class InMemoryEventPublisher : IEventPublisher
{
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(string destination, T message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Broker: {Destination}] Mensaje publicado con éxito: {@Message}", destination, message);
        return Task.CompletedTask;
    }
}
