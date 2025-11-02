using FastPay.Application.Common;
using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Accounts.Mappings;
using FastPay.Application.Accounts.Queries;
using FastPay.Domain.Contracts.Repositories;
using MediatR;

namespace FastPay.Application.Accounts.Handlers;

public sealed class ListAccountsHandler : IRequestHandler<ListAccountsQuery, CommandResult<PagedResult<AccountDto>>>
{
    private readonly IAccountRepository _accountRepository;

    public ListAccountsHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<CommandResult<PagedResult<AccountDto>>> Handle(ListAccountsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.PageSize <= 0 ? 10 : request.PageSize;

        var total = await _accountRepository.CountAsync(request.ClientId);
        var items = await _accountRepository.ListAsync(request.ClientId, page, size);

        var dto = new PagedResult<AccountDto>
        {
            Items = items.Select(a => a.ToAccountDto()).ToList(),
            Page = page,
            PageSize = size,
            Total = total
        };

        return CommandResult<PagedResult<AccountDto>>.Ok(dto, "Contas listadas.");
    }
}

