using System.Reflection;
using System.Threading.Channels;
using FastPay.Application.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace FastPay.Infra.IoC.Events;

public sealed class InMemoryEventBus : BackgroundService, IEventPublisher
{
    private readonly Channel<IEvent> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
    {
        _channel = Channel.CreateUnbounded<IEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(@event, cancellationToken).AsTask();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var evt in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await DispatchAsync(evt, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar evento {EventType}", evt.EventType);
            }
        }
    }

    private async Task DispatchAsync(IEvent evt, CancellationToken ct)
    {
        var eventType = evt.GetType();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var attempt = 0;
            const int maxRetries = 3;
            while (true)
            {
                try
                {
                    var method = handlerType.GetMethod("HandleAsync", BindingFlags.Public | BindingFlags.Instance);
                    if (method is null)
                        throw new MissingMethodException(handlerType.Name, "HandleAsync");
                    var task = (Task)method.Invoke(handler, new object[] { evt, ct })!;
                    await task;
                    break;
                }
                catch (Exception ex) when (attempt++ < maxRetries)
                {
                    var delay = Backoff(attempt);
                    _logger.LogWarning(ex, "Retry {Attempt} ao processar evento {EventType} em {Delay}ms", attempt, evt.EventType, delay.TotalMilliseconds);
                    await Task.Delay(delay, ct);
                }
            }
        }
    }

    private static TimeSpan Backoff(int attempt)
    {
        var baseMs = 100 * Math.Pow(2, attempt - 1);
        var jitter = Random.Shared.Next(0, 100);
        return TimeSpan.FromMilliseconds(baseMs + jitter);
    }
}

