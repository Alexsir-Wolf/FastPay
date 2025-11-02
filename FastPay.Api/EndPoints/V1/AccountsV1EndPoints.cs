using Carter;
using FastPay.Application.Accounts.Commands;
using FastPay.Application.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FastPay.Api.EndPoints.V1;

public class AccountsV1EndPoints : CarterModule
{
    public AccountsV1EndPoints() : base("/api/v1/accounts")
    {
        WithTags("Accounts V1");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateAccount)
            .WithSummary("Cria uma nova conta")
            .WithDescription("Cria uma conta informando clienteId, saldo inicial e limite inicial e moeda.")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        app.MapGet("/{id:int}", GetAccountById)
            .WithSummary("Busca uma conta pelo Id")
            .WithDescription("Busca uma conta pelo seu identificador único.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapGet("/", ListAccounts)
            .WithSummary("Lista contas")
            .WithDescription("Lista contas com filtro opcional por clientId e paginação.")
            .Produces(StatusCodes.Status200OK)
            .WithOpenApi();
    }

    private async Task<IResult> CreateAccount(
        [FromBody] CreateAccountCommand command, 
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(command);

        if (!result.Success)
            return Results.BadRequest(result);

        var location = $"/api/v1/accounts/{result.Data?.Id}";
        return Results.Created(location, result);
    }    

    private static async Task<IResult> GetAccountById(
        [FromRoute] int id,
        [FromServices] IMediator mediator)
    {
        var query = new GetAccountByIdQuery(id);
        var result = await mediator.Send(query);

        if (!result.Success)
            return Results.NotFound(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> ListAccounts(
        [FromQuery] string? clientId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IMediator mediator)
    {
        var query = new ListAccountsQuery(clientId, page, pageSize);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }
}
