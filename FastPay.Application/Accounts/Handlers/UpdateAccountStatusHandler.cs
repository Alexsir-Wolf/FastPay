using FastPay.Application.Accounts.Commands;
using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Accounts.Mappings;
using FastPay.Application.Common;
using FastPay.Domain.Contracts.Repositories;
using Microsoft.Extensions.Logging;
using MediatR;

namespace FastPay.Application.Accounts.Handlers;

public sealed class UpdateAccountStatusHandler : IRequestHandler<UpdateAccountStatusCommand, CommandResult<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<UpdateAccountStatusHandler> _logger;

    public UpdateAccountStatusHandler(IAccountRepository accountRepository, ILogger<UpdateAccountStatusHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<CommandResult<AccountDto>> Handle(
        UpdateAccountStatusCommand request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(
            request.AccountId,
            cancellationToken);

        if (account is null)        
            return CommandResult<AccountDto>.Fail(["Conta não encontrada."]);
        
        account.ChangeStatus(request.Status);

        await _accountRepository.UpdateAsync(
            account,
            cancellationToken);

        await _accountRepository.UnitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Status da conta de {ClientId} alterado para {Status}", account.ClientId, account.Status.ToString());

        var dto = account.ToAccountDto();
        return CommandResult<AccountDto>.Ok(dto, "Conta encontrada.");
    }
}