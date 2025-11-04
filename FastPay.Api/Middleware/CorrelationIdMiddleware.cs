using System.Diagnostics;

namespace FastPay.Api.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
            ? existing.ToString()
            : Activity.Current?.Id ?? Guid.NewGuid().ToString("N");

        context.Response.Headers[HeaderName] = correlationId;

        using var _ = BeginScope(correlationId);
        await _next(context);
    }

    private static IDisposable BeginScope(string correlationId)
    {
        return new CorrelationScope(correlationId);
    }

    private sealed class CorrelationScope : IDisposable
    {
        private readonly string _original;
        public CorrelationScope(string correlationId)
        {
            _original = Activity.Current?.Id ?? string.Empty;
            if (Activity.Current is null)
                Activity.Current = new Activity("request");
            Activity.Current.SetIdFormat(ActivityIdFormat.W3C);
            if (Activity.Current.Id is null)
                Activity.Current.Start();
        }
        public void Dispose()
        {
        }
    }
}

