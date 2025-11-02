using FastPay.Application.Common;
using FastPay.Application.Accounts.Dtos;
using MediatR;

namespace FastPay.Application.Accounts.Commands;

public record CreateAccountCommand(
    string ClientId,
    decimal InitialBalance,
    decimal CreditLimit,
    string Currency) : IRequest<CommandResult<CreateAccountDto>>
{
}
