using FastPay.Application.Common;
using FastPay.Application.Accounts.Dtos;
using MediatR;

namespace FastPay.Application.Accounts.Queries;

public sealed record ListAccountsQuery(
    string? ClientId, 
    int Page = 1, 
    int PageSize = 10) : IRequest<CommandResult<PagedResult<AccountDto>>>;

