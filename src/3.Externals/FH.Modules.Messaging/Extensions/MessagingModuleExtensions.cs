using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.Messaging.Extensions;

public static class MessagingModuleExtensions
{
    public static IServiceCollection AddMessagingModule(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Registro de publicadores de eventos (IEventPublisher) y workers asíncronos de mensajería
        return services;
    }
}
