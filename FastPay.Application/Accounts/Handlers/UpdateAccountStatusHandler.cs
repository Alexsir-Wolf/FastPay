using FastPay.Application.Accounts.Commands;
using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Accounts.Mappings;
using FastPay.Application.Common;
using FastPay.Domain.Contracts.Repositories;
using MediatR;

namespace FastPay.Application.Accounts.Handlers;

public sealed class UpdateAccountStatusHandler : IRequestHandler<UpdateAccountStatusCommand, CommandResult<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;

    public UpdateAccountStatusHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<CommandResult<AccountDto>> Handle(
        UpdateAccountStatusCommand request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId);

        if (account is null)        
            return CommandResult<AccountDto>.Fail(["Conta não encontrada."]);
        
        account.ChangeStatus(request.Status);

        await _accountRepository.UpdateAsync(account);

        await _accountRepository.UnitOfWork.CommitAsync(cancellationToken);

        var dto = account.ToAccountDto();
        return CommandResult<AccountDto>.Ok(dto, "Conta encontrada.");
    }
}