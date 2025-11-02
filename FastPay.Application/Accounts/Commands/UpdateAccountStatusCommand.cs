using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Common;
using FastPay.Domain.Enums;
using MediatR;

namespace FastPay.Application.Accounts.Commands;
public record UpdateAccountStatusCommand(
    int AccountId,
    AccountStatus Status) : IRequest<CommandResult<AccountDto>>;
