using Carter;
using FastPay.Application.Common;
using FastPay.Application.Transactions.Commands;
using FastPay.Application.Transactions.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FastPay.Api.EndPoints.V1;

public sealed class TransactionsV1EndPoints : CarterModule
{
    public TransactionsV1EndPoints() : base("/api/v1/transactions")
    {
        WithTags("Transactions V1");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", Process)
            .WithSummary("Processa uma transação financeira")
            .WithDescription("Processa operações: credit, debit, reserve, capture, reversal, transfer.")
            .Produces<CommandResult<ProcessTransactionResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }

    private static async Task<IResult> Process(
        [FromBody] ProcessTransactionCommand request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        if (!result.Success)
            return Results.BadRequest(result);
        return Results.Ok(result);
    }
}
