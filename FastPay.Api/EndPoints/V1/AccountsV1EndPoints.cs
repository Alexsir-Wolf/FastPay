using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FastPay.Application.Accounts.Commands;

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
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
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
}
