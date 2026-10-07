using FH.Customers.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace FH.Customers.Infrastructure.Messaging;

/// <summary>
/// Publicador de eventos en memoria: registra el mensaje en el log en lugar de enviarlo a un broker.
/// Es el reemplazo de desarrollo de Kafka; basta con registrar otra implementación de
/// <see cref="IEventPublisher"/> para publicar de verdad.
/// </summary>
public class InMemoryEventPublisher : IEventPublisher
{
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(string destination, T message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Broker: {Destination}] Mensaje publicado: {@Message}", destination, message);
        return Task.CompletedTask;
    }
}