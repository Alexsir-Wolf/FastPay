using Carter;
using FastPay.Infra.Data.Persistence;

namespace FastPay.Api.EndPoints;

public sealed class HealthEndPoints : CarterModule
{
    public HealthEndPoints() : base("/health")
    {
        WithTags("Health");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/live", () => Results.Ok())
            .WithSummary("API check")
            .WithDescription("Indica se a aplicação está em execução.")
            .Produces(StatusCodes.Status200OK)
            .WithOpenApi();

        app.MapGet("/ready", async (FastPayDbContext db, CancellationToken ct) =>
        {
            var can = await db.Database.CanConnectAsync(ct);
            return can ? Results.Ok() : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        })
            .WithSummary("DB check")
            .WithDescription("Indica se as banco de dados está pronto.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .WithOpenApi();
    }
}

