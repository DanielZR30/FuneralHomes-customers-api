namespace FH.Customers.Application.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string destination, T message, CancellationToken cancellationToken = default);
}
