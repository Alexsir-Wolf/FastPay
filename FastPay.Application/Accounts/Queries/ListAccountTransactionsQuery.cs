using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Common;
using MediatR;

namespace FastPay.Application.Accounts.Queries;

public sealed record ListAccountTransactionsQuery(
    int AccountId,
    int Page = 1,
    int PageSize = 10) : IRequest<CommandResult<PagedResult<TransactionDto>>>;

