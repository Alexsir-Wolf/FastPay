using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Accounts.Mappings;
using FastPay.Application.Accounts.Queries;
using FastPay.Application.Common;
using FastPay.Domain.Contracts.Repositories;
using MediatR;

namespace FastPay.Application.Accounts.Handlers;

public sealed class GetAccountByClientIdHandler : IRequestHandler<GetAccountByClientIdQuery, CommandResult<IEnumerable<AccountDto>>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByClientIdHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<CommandResult<IEnumerable<AccountDto>>> Handle(
        GetAccountByClientIdQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetByClientIdAsync(
            request.clientId, 
            cancellationToken);

        if (accounts is null || !accounts.Any())
            return CommandResult<IEnumerable<AccountDto>>.Fail(["Conta não encontrada."]);

        var dtos = accounts.Select(account => account.ToAccountDto());

        return CommandResult<IEnumerable<AccountDto>>.Ok(dtos);
    }
}
