using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Accounts.Mappings;
using FastPay.Application.Accounts.Queries;
using FastPay.Application.Common;
using FastPay.Domain.Contracts.Repositories;
using MediatR;

namespace FastPay.Application.Accounts.Handlers;

public sealed class ListAccountTransactionsHandler : IRequestHandler<ListAccountTransactionsQuery, CommandResult<PagedResult<TransactionDto>>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;

    public ListAccountTransactionsHandler(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
    }

    public async Task<CommandResult<PagedResult<TransactionDto>>> Handle(
        ListAccountTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
            return CommandResult<PagedResult<TransactionDto>>.Fail(["Conta não encontrada."]);

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.PageSize <= 0 ? 10 : request.PageSize;

        var total = await _transactionRepository.CountByAccountAsync(request.AccountId, cancellationToken);
        var items = await _transactionRepository.ListByAccountAsync(request.AccountId, page, size, cancellationToken);

        var dto = new PagedResult<TransactionDto>
        {
            Items = items.Select(t => t.ToTransactionDto()).ToList(),
            Page = page,
            PageSize = size,
            Total = total
        };

        return CommandResult<PagedResult<TransactionDto>>.Ok(dto);
    }
}

