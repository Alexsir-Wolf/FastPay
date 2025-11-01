using FastPay.Application.Common;
using FastPay.Application.Accounts.Dtos;
using MediatR;

namespace FastPay.Application.Accounts.Commands;

public record CreateAccountCommand(
    int ClientId,
    decimal InitialBalance,
    decimal CreditLimit) : IRequest<CommandResult<AccountDto>>
{
}
