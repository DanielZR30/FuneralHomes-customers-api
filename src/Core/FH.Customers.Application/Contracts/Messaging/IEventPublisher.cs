namespace FH.Customers.Application.Contracts.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string destination, T message, CancellationToken cancellationToken = default);
}
