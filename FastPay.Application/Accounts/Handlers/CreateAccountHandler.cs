using FastPay.Application.Common;
using FastPay.Application.Accounts.Dtos;
using FastPay.Application.Accounts.Mappings;
using FastPay.Domain.Contracts.Repositories;
using FastPay.Domain.Entities;
using FastPay.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using FastPay.Application.Accounts.Commands;
using FastPay.Domain.Enums;

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

    public async Task<CommandResult<CreateAccountDto>> Handle(
        CreateAccountCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando nova conta para o cliente {ClientId}", request.ClientId);

        var existing = await _accountRepository.GetByClientAndCurrencyAsync(
            request.ClientId, 
            request.Currency, 
            cancellationToken);

        if (existing is not null)
        { 
            if (existing.Status != AccountStatus.Blocked)            
                return CommandResult<CreateAccountDto>.Fail(new[] { $"Já existe uma conta ativa para esta moeda." });            
        }

        var account = new Account(
            clientId: request.ClientId,
            currency: request.Currency,
            initialBalance: new Money(request.InitialBalance),
            creditLimit: new Money(request.CreditLimit)
        );

        await _accountRepository.AddAsync(account, cancellationToken);

        await _accountRepository.UnitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Conta criada com sucesso para o cliente {ClientId} (AccountId: {AccountId})",
            request.ClientId, account.Id);

        var dto = account.ToCreateAccountDto();

        return CommandResult<CreateAccountDto>.Ok(dto);
    }
}
