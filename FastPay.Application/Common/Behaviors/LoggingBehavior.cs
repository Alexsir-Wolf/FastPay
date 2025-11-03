using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FastPay.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public LoggingBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestName"] = requestName
        });

        _logger.LogInformation("Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();

            _logger.LogInformation(
                "Handled {RequestName} em {ElapsedMs} ms",
                requestName,
                sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "Error handling {RequestName} depois de {ElapsedMs} ms",
                requestName,
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}

