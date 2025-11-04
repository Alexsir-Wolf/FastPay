using Carter;
using FastPay.Application.Accounts.Commands;
using FastPay.Application.Accounts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Common;

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

        app.MapGet("/{clientId}", GetAccountByClientId)
            .WithSummary("Busca uma conta pelo Id do Cliente")
            .WithDescription("Busca uma conta pelo Id do Cliente.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapGet("/", ListAccounts)
            .WithSummary("Lista contas")
            .WithDescription("Lista contas com filtro opcional por clientId e paginação.")
            .Produces(StatusCodes.Status200OK)
            .WithOpenApi();

        app.MapPut("/{id:int}/status", UpdateAccountStatus)
            .WithSummary("Atualiza o status da conta")
            .WithDescription("Atualiza o status (Active, Blocked, Inactive) da conta informada.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapGet("/{id:int}/transactions", GetAccountTransactions)
            .WithSummary("Lista o histórico de transações da conta")
            .WithDescription("Retorna o histórico paginado de transações da conta informada.")
            .Produces<CommandResult<PagedResult<TransactionDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }

    private async Task<IResult> CreateAccount(
        [FromBody] CreateAccountCommand command, 
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.Success)
            return Results.BadRequest(result);

        var location = $"/api/v1/accounts/{result.Data?.Id}";
        return Results.Created(location, result);
    }    

    private static async Task<IResult> GetAccountById(
        [FromRoute] int id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAccountByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Success)
            return Results.NotFound(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetAccountByClientId(
        [FromRoute] string clientId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAccountByClientIdQuery(clientId);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Success)
            return Results.NotFound(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> ListAccounts(
        [FromQuery] string? clientId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new ListAccountsQuery(clientId, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateAccountStatus(
        [FromRoute] int id,
        [FromBody] UpdateAccountStatusRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken) 
    {  

        var command = new UpdateAccountStatusCommand(id, request.Status);
        var result = await mediator.Send(command, cancellationToken);
       
        if (!result.Success)
            return Results.BadRequest(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetAccountTransactions(
        [FromRoute] int id,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new ListAccountTransactionsQuery(id, page, pageSize);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.Success && (result.Errors?.Any(e => e.Contains("Conta não encontrada", StringComparison.OrdinalIgnoreCase)) ?? false))
            return Results.NotFound(result);

        return Results.Ok(result);
    }
}
