using FastPay.Application.Common;
using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Accounts.Mappings;
using FastPay.Application.Accounts.Queries;
using FastPay.Domain.Contracts.Repositories;
using MediatR;

namespace FastPay.Application.Accounts.Handlers;

public sealed class GetAccountByIdHandler : IRequestHandler<GetAccountByIdQuery, CommandResult<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByIdHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<CommandResult<AccountDto>> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId);

        if (account is null)        
            return CommandResult<AccountDto>.Fail(["Conta não encontrada."]);        

        var dto = account.ToAccountDto();
        return CommandResult<AccountDto>.Ok(dto, "Conta encontrada.");
    }
}
