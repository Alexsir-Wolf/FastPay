using System.Text.Json;
using FastPay.Application.Common;
using FastPay.Application.Transactions.Commands;
using FastPay.Application.Transactions.Dtos;
using FastPay.Application.Common.Events;
using FastPay.Application.Transactions.Events;
using FastPay.Domain.Common;
using FastPay.Domain.Contracts.Repositories;
using FastPay.Domain.Entities;
using FastPay.Domain.Constants;
using FastPay.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using MediatR;

namespace FastPay.Application.Transactions.Handlers;

public sealed class ProcessTransactionHandler : IRequestHandler<ProcessTransactionCommand, CommandResult<ProcessTransactionResponse>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<ProcessTransactionHandler> _logger;
    private readonly IEventPublisher _eventPublisher;

    public ProcessTransactionHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ILogger<ProcessTransactionHandler> logger,
        IEventPublisher eventPublisher)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
        _eventPublisher = eventPublisher;
    }

    public async Task<CommandResult<ProcessTransactionResponse>> Handle(
        ProcessTransactionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.Equals(request.Operation, TransactionOperations.Transfer, StringComparison.OrdinalIgnoreCase))
                return await HandleTransferAsync(request, cancellationToken);

            ProcessTransactionResponse? response = null;
            TransactionProcessedEvent? evt = null;

            await _accountRepository.UnitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var account = await _accountRepository.GetByIdForUpdateAsync(request.SourceAccountId, ct);
                if (account is null)
                    throw new DomainException("Conta não encontrada.");

                if (!string.Equals(account.Currency, request.Currency, StringComparison.OrdinalIgnoreCase))
                    throw new DomainException("Moeda da conta diferente da operação.");

                var existing = await _transactionRepository.GetByReferenceAsync(
                    request.SourceAccountId,
                    request.ReferenceId,
                    request.Operation,
                    ct);

                if (existing is not null)
                {
                    response = BuildResponse(account, existing);
                    return;
                }

                var money = Money.FromCents(request.Amount);

                var tx = new Transaction(
                    accountId: request.SourceAccountId,
                    amount: money.Amount,
                    currency: request.Currency,
                    operation: request.Operation,
                    referenceId: request.ReferenceId,
                    metadataJson: request.Metadata is null ? null : JsonSerializer.Serialize(request.Metadata));

                await _transactionRepository.AddAsync(tx, ct);

                try
                {
                    ApplyOperation(account, request.Operation, money);
                    tx.MarkSuccess();
                }
                catch (DomainException dex)
                {
                    tx.MarkFailed(dex.Message);
                }

                await _accountRepository.UpdateAsync(account, ct);

                response = BuildResponse(account, tx);
                evt = new TransactionProcessedEvent(
                    ReferenceId: tx.ReferenceId,
                    Operation: tx.Operation,
                    Status: tx.Status,
                    AccountId: account.Id,
                    DestinationAccountId: null,
                    Amount: tx.Amount,
                    Currency: tx.Currency,
                    AvailableBalance: account.AvailableBalance.Amount,
                    ReservedBalance: account.ReservedBalance.Amount,
                    UsedCredit: account.UsedCredit.Amount,
                    Timestamp: tx.Timestamp,
                    MetadataJson: tx.MetadataJson);
            }, cancellationToken: cancellationToken);

            if (response is null)
                return CommandResult<ProcessTransactionResponse>.Fail("Falha ao processar a transação.");

            if (evt is not null)
                await _eventPublisher.PublishAsync(evt, cancellationToken);

            return CommandResult<ProcessTransactionResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar transação {ReferenceId}", request.ReferenceId);
            return CommandResult<ProcessTransactionResponse>.Fail("Erro interno ao processar a transação.");
        }
    }

    private async Task<CommandResult<ProcessTransactionResponse>> HandleTransferAsync(
        ProcessTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var sourceId = request.SourceAccountId;
        var destId = request.DestinationAccountId;

        if (sourceId == destId)
            return CommandResult<ProcessTransactionResponse>.Fail("Conta de origem e destino não podem ser iguais.");

        ProcessTransactionResponse? response = null;
        TransactionProcessedEvent? evt = null;

        await _accountRepository.UnitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var first = Math.Min(sourceId, destId);
            var second = Math.Max(sourceId, destId);

            var firstAcc = await _accountRepository.GetByIdForUpdateAsync(first, ct);
            var secondAcc = await _accountRepository.GetByIdForUpdateAsync(second, ct);

            var source = sourceId == first ? firstAcc : secondAcc;
            var dest = destId == second ? secondAcc : firstAcc;

            if (source is null || dest is null)
                throw new DomainException("Conta de origem ou destino não encontrada.");

            if (!string.Equals(source.Currency, request.Currency, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(dest.Currency, request.Currency, StringComparison.OrdinalIgnoreCase))
                throw new DomainException("Moeda da operação diferente das contas.");

            var existing = await _transactionRepository.GetByReferenceAsync(
                sourceId,
                request.ReferenceId,
                TransactionOperations.Transfer,
                ct);

            if (existing is not null)
            {
                response = BuildResponse(source, existing);
                return;
            }

            var money = Money.FromCents(request.Amount);

            var tx = new Transaction(
                accountId: sourceId,
                amount: money.Amount,
                currency: request.Currency,
                operation: TransactionOperations.Transfer,
                referenceId: request.ReferenceId,
                destinationAccountId: destId,
                metadataJson: request.Metadata is null ? null : JsonSerializer.Serialize(request.Metadata));

            await _transactionRepository.AddAsync(tx, ct);

            try
            {
                source.Debit(money);
                dest.Credit(money);
                tx.MarkSuccess();
            }
            catch (DomainException dex)
            {
                tx.MarkFailed(dex.Message);
            }

            await _accountRepository.UpdateAsync(source, ct);
            await _accountRepository.UpdateAsync(dest, ct);

            response = BuildResponse(source, tx);
            evt = new TransactionProcessedEvent(
                ReferenceId: tx.ReferenceId,
                Operation: tx.Operation,
                Status: tx.Status,
                AccountId: source.Id,
                DestinationAccountId: dest.Id,
                Amount: tx.Amount,
                Currency: tx.Currency,
                AvailableBalance: source.AvailableBalance.Amount,
                ReservedBalance: source.ReservedBalance.Amount,
                UsedCredit: source.UsedCredit.Amount,
                Timestamp: tx.Timestamp,
                MetadataJson: tx.MetadataJson);
        }, cancellationToken: cancellationToken);

        if (response is null)
            return CommandResult<ProcessTransactionResponse>.Fail("Falha ao processar a transação.");

        if (evt is not null)
            await _eventPublisher.PublishAsync(evt, cancellationToken);

        return CommandResult<ProcessTransactionResponse>.Ok(response);
    }
    
    private static void ApplyOperation(Account account, string operation, Money amount)
    {
        switch (operation.ToLowerInvariant())
        {
            case "credit":
                account.Credit(amount);
                break;
            case "debit":
                account.Debit(amount);
                break;
            case "reserve":
                account.Reserve(amount);
                break;
            case "capture":
                account.Capture(amount);
                break;
            case "reversal":
                account.ReverseDebit(amount);
                break;
            default:
                throw new DomainException("Operação não suportada.");
        }
    }

    private static ProcessTransactionResponse BuildResponse(Account account, Transaction tx)
    {
        var balanceCents = account.AvailableBalance.ToCents()
            + account.ReservedBalance.ToCents()
            - account.UsedCredit.ToCents();

        return new ProcessTransactionResponse
        {
            TransactionId = $"{tx.ReferenceId}-PROCESSED",
            Status = tx.Status,
            Balance = balanceCents,
            ReservedBalance = account.ReservedBalance.ToCents(),
            AvailableBalance = account.AvailableBalance.ToCents(),
            Timestamp = tx.Timestamp,
            ErrorMessage = tx.ErrorMessage
        };
    }
}
