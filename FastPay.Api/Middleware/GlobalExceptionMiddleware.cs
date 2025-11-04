using System.Net;
using System.Text.Json;
using FastPay.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FastPay.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var status = MapStatusCode(ex);
        var title = GetTitle(status);

        _logger.LogError(ex, "Erro não tratado. Status={StatusCode}", status);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new
        {
            type = "about:blank",
            title,
            status = (int)status,
            detail = ex is DomainException ? ex.Message : "Erro interno no servidor.",
            traceId = context.TraceIdentifier,
            correlationId = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString()
        };

        var json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }

    private static HttpStatusCode MapStatusCode(Exception ex)
    {
        return ex switch
        {
            DomainException => HttpStatusCode.UnprocessableEntity,
            DbUpdateConcurrencyException => HttpStatusCode.Conflict,
            PostgresException pex when pex.SqlState 
                is PostgresErrorCodes.SerializationFailure 
                or PostgresErrorCodes.DeadlockDetected
                => HttpStatusCode.Conflict,
            TimeoutException => HttpStatusCode.RequestTimeout,
            _ => HttpStatusCode.InternalServerError
        };
    }

    private static string GetTitle(HttpStatusCode code) => code switch
    {
        HttpStatusCode.UnprocessableEntity => "Erro de regra de negócio",
        HttpStatusCode.Conflict => "Conflito de concorrência",
        HttpStatusCode.RequestTimeout => "Tempo de requisição excedido",
        HttpStatusCode.NotFound => "Recurso não encontrado",
        _ => "Erro interno"
    };
}

