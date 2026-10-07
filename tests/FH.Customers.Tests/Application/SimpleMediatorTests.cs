using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using FH.Customers.Application.Common;
using FH.Customers.Application.Events;
using FH.Customers.Application.Mediator;
using FH.Customers.Domain.Common;
using FH.Customers.Domain.Exceptions;

namespace FH.Customers.Tests.Application;

public class SimpleMediatorTests
{
    // ---- petición de prueba ----
    private sealed record PingCommand(string Text) : IRequest<Result<string>>;

    private sealed class PingValidator : AbstractValidator<PingCommand>
    {
        public PingValidator() => RuleFor(x => x.Text).NotEmpty().WithMessage("El texto es obligatorio.");
    }

    private sealed class PingHandler : IRequestHandler<PingCommand, Result<string>>
    {
        public static int Calls;

        public Task<Result<string>> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            Calls++;
            if (request.Text == "boom")
                throw new BusinessRuleException("Regla violada.");

            return Task.FromResult(Result<string>.Success($"pong:{request.Text}"));
        }
    }

    private sealed record NoHandlerCommand : IRequest<Result>;

    private static IMediator BuildMediator(bool withValidator = true)
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<PingCommand, Result<string>>, PingHandler>();
        if (withValidator)
            services.AddScoped<IValidator<PingCommand>, PingValidator>();

        var provider = services.BuildServiceProvider();
        return new SimpleMediator(provider, NullLogger<SimpleMediator>.Instance);
    }

    [Fact]
    public async Task Send_WhenRequestIsValid_ShouldCallHandler()
    {
        PingHandler.Calls = 0;

        var result = await BuildMediator().Send(new PingCommand("hola"));

        Assert.True(result.IsSuccess);
        Assert.Equal("pong:hola", result.Value);
        Assert.Equal(1, PingHandler.Calls);
    }

    [Fact]
    public async Task Send_WhenValidationFails_ShouldReturnBadRequestAndNotCallHandler()
    {
        PingHandler.Calls = 0;

        var result = await BuildMediator().Send(new PingCommand(""));

        Assert.True(result.IsFailure);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("El texto es obligatorio.", result.ErrorMessage);
        Assert.Equal(0, PingHandler.Calls);
    }

    [Fact]
    public async Task Send_WhenThereIsNoValidator_ShouldGoStraightToHandler()
    {
        var result = await BuildMediator(withValidator: false).Send(new PingCommand(""));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Send_WhenHandlerThrowsBusinessRule_ShouldPropagateOriginalException()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => BuildMediator().Send(new PingCommand("boom")));
    }

    [Fact]
    public async Task Send_WhenThereIsNoHandler_ShouldThrowMediatorException()
    {
        await Assert.ThrowsAsync<MediatorException>(
            () => BuildMediator().Send(new NoHandlerCommand()));
    }

    // ---- eventos de dominio ----
    private sealed record SomethingHappened() : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => nameof(SomethingHappened);
    }

    private sealed class FirstHandler : IDomainEventHandler<SomethingHappened>
    {
        public static int Calls;
        public Task Handle(SomethingHappened domainEvent, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.CompletedTask;
        }
    }

    private sealed class SecondHandler : IDomainEventHandler<SomethingHappened>
    {
        public static int Calls;
        public Task Handle(SomethingHappened domainEvent, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Dispatcher_ShouldCallEveryRegisteredHandler()
    {
        FirstHandler.Calls = 0;
        SecondHandler.Calls = 0;

        var services = new ServiceCollection();
        services.AddScoped<IDomainEventHandler<SomethingHappened>, FirstHandler>();
        services.AddScoped<IDomainEventHandler<SomethingHappened>, SecondHandler>();
        var dispatcher = new DomainEventDispatcher(services.BuildServiceProvider());

        await dispatcher.DispatchAsync(new SomethingHappened());

        Assert.Equal(1, FirstHandler.Calls);
        Assert.Equal(1, SecondHandler.Calls);
    }

    [Fact]
    public async Task Dispatcher_WhenNoHandlers_ShouldDoNothing()
    {
        var dispatcher = new DomainEventDispatcher(new ServiceCollection().BuildServiceProvider());

        await dispatcher.DispatchAsync(new SomethingHappened());
    }
}
