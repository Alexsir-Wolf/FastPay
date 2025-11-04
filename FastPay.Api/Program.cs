using Carter;
using FastPay.Infra.IoC;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Serilog;
using FastPay.Api.Middleware;
using Prometheus;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("FastPay API - Iniciado");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) =>
    {
        cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();

        var pgConn = ctx.Configuration["Settings:PostgresSettings:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(pgConn))
        {
            cfg.WriteTo.PostgreSQL(
                connectionString: pgConn,
                tableName: "fast_pay_logs",
                needAutoCreateTable: true
            );
        }
    });

    // Services
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddCarter();
    builder.Services.AddHealthChecks();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "FastPay API",
            Version = "v1",
            Description = "API financeira do FastPay - operações atômicas e seguras."
        });
    });

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    var app = builder.Build();

    // Global middlewares
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagCtx, http) =>
        {
            diagCtx.Set("TraceIdentifier", http.TraceIdentifier);
            diagCtx.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FastPay API v1");
            c.RoutePrefix = "swagger";
        });
    }

    // Prometheus
    app.UseHttpMetrics();
    app.MapMetrics();

    app.UseHttpsRedirection();
    app.MapCarter();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FastPay API - Crashou");
}
finally
{
    Log.CloseAndFlush();
}