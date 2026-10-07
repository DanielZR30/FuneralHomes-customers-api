using MediatR;
using Microsoft.Extensions.Logging;

namespace FH.Shared.Application.Behaviors;

/// <summary>
/// Pipeline behavior de MediatR para registrar la ejecución y tiempo de respuesta de comandos y consultas.
/// </summary>
/// <typeparam name="TRequest">Tipo de la petición CQRS.</typeparam>
/// <typeparam name="TResponse">Tipo de la respuesta.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Iniciando ejecución de la solicitud: {RequestName}", requestName);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Solicitud {RequestName} completada con éxito en {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error durante la ejecución de la solicitud {RequestName} tras {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
