using FastPay.Application.Common;
using FastPay.Application.Accounts.Dtos;
using MediatR;

namespace FastPay.Application.Accounts.Queries;

public sealed record GetAccountByClientIdQuery(string clientId) : IRequest<CommandResult<IEnumerable<AccountDto>>>;
