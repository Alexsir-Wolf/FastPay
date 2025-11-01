using FastPay.Application.Common;
using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Accounts.Mappings;
using FastPay.Domain.Contracts.Repositories;
using FastPay.Domain.Entities;
using FastPay.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using FastPay.Application.Accounts.Commands;

namespace FastPay.Application.Accounts.Handlers;

public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, CommandResult<CreateAccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CreateAccountHandler> _logger;

    public CreateAccountHandler(IAccountRepository accountRepository, ILogger<CreateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<CommandResult<CreateAccountDto>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando nova conta para o cliente {ClientId}", request.ClientId);

        var account = new Account(
            clientId: request.ClientId,
            initialBalance: new Money(request.InitialBalance),
            creditLimit: new Money(request.CreditLimit)
        );

        await _accountRepository.AddAsync(account);

        await _accountRepository.UnitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Conta criada com sucesso para o cliente {ClientId} (AccountId: {AccountId})",
            request.ClientId, account.Id);

        var dto = account.ToAccountDto();

        return CommandResult<CreateAccountDto>.Ok(dto, "Conta criada com sucesso.");
    }
}
