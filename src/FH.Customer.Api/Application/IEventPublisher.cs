namespace FH.Messaging.Application;

public interface IEventPublisher
{
    Task PublishAsync<T>(string destination, T message, CancellationToken cancellationToken = default);
}
