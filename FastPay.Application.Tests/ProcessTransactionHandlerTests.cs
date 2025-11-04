using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastPay.Application.Common.Events;
using FastPay.Application.Transactions.Commands;
using FastPay.Application.Transactions.Events;
using FastPay.Application.Transactions.Handlers;
using FastPay.Domain.Contracts.Repositories;
using FastPay.Domain.Entities;
using FastPay.Domain.Enums;
using FastPay.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace FastPay.Application.Tests;

public class ProcessTransactionHandlerTests
{
    private readonly IAccountRepository _accountRepo = Substitute.For<IAccountRepository>();
    private readonly ITransactionRepository _txRepo = Substitute.For<ITransactionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly ILogger<ProcessTransactionHandler> _logger = Substitute.For<ILogger<ProcessTransactionHandler>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    public ProcessTransactionHandlerTests()
    {
        _accountRepo.UnitOfWork.Returns(_uow);
        _uow.ExecuteInTransactionAsync(
            Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<IsolationLevel>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var fn = call.Arg<Func<CancellationToken, Task>>();
                return fn(CancellationToken.None);
            });
    }

    [Fact(DisplayName = "credit: sucesso")]
    public async Task Credit_Succeeds()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        _txRepo.GetByReferenceAsync(1, "TXN-CR-01", "credit", Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("credit", 1, 0, 2500, "BRL", "TXN-CR-01", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(TransactionStatus.Success, result.Data!.Status);
        Assert.Null(result.Data.ErrorMessage);
    }

    [Fact(DisplayName = "debit: usa limite quando precisa")]
    public async Task Debit_Uses_Credit()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        _txRepo.GetByReferenceAsync(1, "TXN-DB-01", "debit", Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("debit", 1, 0, 14000, "BRL", "TXN-DB-01", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(TransactionStatus.Success, result.Data!.Status);
    }

    [Fact(DisplayName = "capture: falha quando reservado insuficiente")]
    public async Task Capture_Fails_When_Insufficient_Reserved()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        _txRepo.GetByReferenceAsync(1, "TXN-CP-01", "capture", Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("capture", 1, 0, 3000, "BRL", "TXN-CP-01", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(TransactionStatus.Failed, result.Data!.Status);
        Assert.NotNull(result.Data.ErrorMessage);
    }

    [Fact(DisplayName = "transfer: falha se mesma conta")]
    public async Task Transfer_Fails_When_Same_Account()
    {
        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("transfer", 1, 1, 1000, "BRL", "TXN-TR-ERR-01", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors!.Any());
    }

    [Fact(DisplayName = "transfer: sucesso")]
    public async Task Transfer_Succeeds()
    {
        var a1 = new Account("C-1", "BRL", new Money(100), new Money(50));
        var a2 = new Account("C-2", "BRL", new Money(10), new Money(0));

        _accountRepo.GetByIdForUpdateAsync(Arg.Is<int>(i => i == 1), Arg.Any<CancellationToken>()).Returns(a1);
        _accountRepo.GetByIdForUpdateAsync(Arg.Is<int>(i => i == 2), Arg.Any<CancellationToken>()).Returns(a2);
        _txRepo.GetByReferenceAsync(1, "TXN-TR-01", "transfer", Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("transfer", 1, 2, 1500, "BRL", "TXN-TR-01", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(TransactionStatus.Success, result.Data!.Status);
    }

    [Fact(DisplayName = "idempotency: retorna existente")]
    public async Task Returns_Existing_When_Duplicate_Reference()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));
        var existing = new Transaction(1, 10m, "BRL", "credit", "TXN-CR-EXIST");
        existing.MarkSuccess();

        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        _txRepo.GetByReferenceAsync(1, "TXN-CR-EXIST", "credit", Arg.Any<CancellationToken>()).Returns(existing);

        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("credit", 1, 0, 1000, "BRL", "TXN-CR-EXIST", null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("TXN-CR-EXIST-PROCESSED", result.Data!.TransactionId);
        Assert.Equal(TransactionStatus.Success, result.Data.Status);
    }

    [Fact(DisplayName = "reserve: sucesso")]
    public async Task Reserve_Succeeds()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        _txRepo.GetByReferenceAsync(1, "TXN-RS-01", "reserve", Arg.Any<CancellationToken>()).Returns((Transaction?)null);

        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("reserve", 1, 0, 3000, "BRL", "TXN-RS-01", null);

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(TransactionStatus.Success, result.Data!.Status);
        await _publisher.Received(1).PublishAsync(Arg.Any<TransactionProcessedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "reserve/capture: captura parcial com sucesso")]
    public async Task Reserve_Capture_Partial_Succeeds()
    {
        var account = new Account("C-1", "BRL", new Money(50), new Money(0));
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        _txRepo.GetByReferenceAsync(1, "TXN-RS-02", "reserve", Arg.Any<CancellationToken>()).Returns((Transaction?)null);
        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var reserve = new ProcessTransactionCommand("reserve", 1, 0, 4000, "BRL", "TXN-RS-02", null);
        var rs = await handler.Handle(reserve, CancellationToken.None);
        Assert.True(rs.Success);
        // captura parcial
        _txRepo.GetByReferenceAsync(1, "TXN-CP-02", "capture", Arg.Any<CancellationToken>()).Returns((Transaction?)null);
        var capture = new ProcessTransactionCommand("capture", 1, 0, 1000, "BRL", "TXN-CP-02", null);
        var cp = await handler.Handle(capture, CancellationToken.None);
        Assert.True(cp.Success);
        Assert.Equal(TransactionStatus.Success, cp.Data!.Status);
    }

    [Fact(DisplayName = "currency mismatch: falha")]
    public async Task Currency_Mismatch_Fails()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        _txRepo.GetByReferenceAsync(1, "TXN-CR-CURR", "credit", Arg.Any<CancellationToken>()).Returns((Transaction?)null);
        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("credit", 1, 0, 1000, "USD", "TXN-CR-CURR", null);
        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.False(result.Success);
    }

    [Fact(DisplayName = "account not found: falha")]
    public async Task Account_Not_Found_Fails()
    {
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns((Account?)null);
        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("credit", 1, 0, 1000, "BRL", "TXN-ACC-NF", null);
        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.False(result.Success);
    }

    [Fact(DisplayName = "transfer currency mismatch: falha")]
    public async Task Transfer_Currency_Mismatch_Fails()
    {
        var a1 = new Account("C-1", "BRL", new Money(100), new Money(0));
        var a2 = new Account("C-2", "BRL", new Money(100), new Money(0));
        _accountRepo.GetByIdForUpdateAsync(Arg.Is<int>(i => i == 1), Arg.Any<CancellationToken>()).Returns(a1);
        _accountRepo.GetByIdForUpdateAsync(Arg.Is<int>(i => i == 2), Arg.Any<CancellationToken>()).Returns(a2);
        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("transfer", 1, 2, 1000, "USD", "TXN-TR-CURR", null);
        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.False(result.Success);
    }

    [Fact(DisplayName = "debit: falha por insuficiência de saldo+limite")]
    public async Task Debit_Fails_When_Insufficient_Total()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("debit", 1, 0, 20000, "BRL", "TXN-DB-NO", null);
        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.True(result.Success);
        Assert.Equal(TransactionStatus.Failed, result.Data!.Status);
    }

    [Fact(DisplayName = "event: publicado em sucesso")]
    public async Task Publishes_Event_On_Success()
    {
        var account = new Account("C-1", "BRL", new Money(10), new Money(0));
        _accountRepo.GetByIdForUpdateAsync(1, Arg.Any<CancellationToken>()).Returns(account);
        var handler = new ProcessTransactionHandler(_accountRepo, _txRepo, _logger, _publisher);
        var cmd = new ProcessTransactionCommand("credit", 1, 0, 1000, "BRL", "TXN-EVT-1", null);
        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.True(result.Success);
        await _publisher.Received(1).PublishAsync(Arg.Any<TransactionProcessedEvent>(), Arg.Any<CancellationToken>());
    }
}
