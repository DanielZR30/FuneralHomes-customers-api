using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FH.Customers.Application.Common;

namespace FH.Customers.Application.Mediator;

/// <summary>
/// Mediador propio (sin librerías de terceros). Por cada petición:
/// 1) registra el inicio, 2) ejecuta los validadores de FluentValidation (si hay),
/// 3) busca por reflexión el IRequestHandler registrado y lo invoca, 4) registra la duración.
/// Si la validación falla, no se llama al handler y se devuelve un Result de fallo (400).
/// </summary>
public class SimpleMediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SimpleMediator> _logger;

    public SimpleMediator(IServiceProvider serviceProvider, ILogger<SimpleMediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var requestName = requestType.Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Iniciando ejecución de la solicitud: {RequestName}", requestName);

        try
        {
            // 1. Validación (FluentValidation)
            var failures = await ValidateAsync(request, requestType, cancellationToken);

            if (failures.Count > 0)
            {
                var message = string.Join(" ", failures.Select(f => f.ErrorMessage).Distinct());

                _logger.LogWarning("Solicitud {RequestName} rechazada por validación: {Message}", requestName, message);

                return CreateValidationFailure<TResponse>(message, failures);
            }

            // 2. Handler
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new MediatorException($"No se encontró un handler para {requestName}");

            var method = handlerType.GetMethod("Handle")!;

            Task<TResponse> task;
            try
            {
                task = (Task<TResponse>)method.Invoke(handler, new object[] { request, cancellationToken })!;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }

            var response = await task;

            _logger.LogInformation(
                "Solicitud {RequestName} completada en {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la solicitud {RequestName} tras {ElapsedMilliseconds} ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<List<FluentValidation.Results.ValidationFailure>> ValidateAsync(
        object request,
        Type requestType,
        CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        var validators = _serviceProvider.GetServices(validatorType);

        var failures = new List<FluentValidation.Results.ValidationFailure>();
        var context = new ValidationContext<object>(request);

        foreach (var validator in validators)
        {
            var result = await ((IValidator)validator!).ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors);
        }

        return failures;
    }

    // Convierte el fallo de validación en el Result que espera el controlador (400 + mensaje).
    private static TResponse CreateValidationFailure<TResponse>(
        string message,
        List<FluentValidation.Results.ValidationFailure> failures)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(message, "Validation", 400);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failure = responseType.GetMethod(
                "Failure",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                null,
                new[] { typeof(string), typeof(string), typeof(int) },
                null)!;

            return (TResponse)failure.Invoke(null, new object?[] { message, "Validation", 400 })!;
        }

        // La respuesta no es un Result: se informa con excepción de validación.
        throw new ValidationException(failures);
    }
}
