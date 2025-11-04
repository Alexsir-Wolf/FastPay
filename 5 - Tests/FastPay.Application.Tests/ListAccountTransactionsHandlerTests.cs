using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using FastPay.Application.Accounts.Handlers;
using FastPay.Application.Accounts.Queries;
using FastPay.Domain.Contracts.Repositories;
using FastPay.Domain.Entities;
using NSubstitute;
using Xunit;
using FastPay.Domain.ValueObjects;

namespace FastPay.Application.Tests;

public class ListAccountTransactionsHandlerTests
{
    private readonly ITransactionRepository _txRepo = Substitute.For<ITransactionRepository>();
    private readonly IAccountRepository _accountRepo = Substitute.For<IAccountRepository>();

    [Fact]
    public async Task NotFound_When_Account_Missing()
    {
        _accountRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Account?)null);
        var h = new ListAccountTransactionsHandler(_txRepo, _accountRepo);
        var result = await h.Handle(new ListAccountTransactionsQuery(1, 1, 10), CancellationToken.None);
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public async Task Returns_Paged_Transactions()
    {
        var acc = new Account("C-1", "BRL", new Money(0), new Money(0));
        _accountRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(acc);

        var list = new List<Transaction>
        {
            new Transaction(1, 10, "BRL", "credit", "R1"),
            new Transaction(1, 20, "BRL", "debit", "R2"),
            new Transaction(1, 30, "BRL", "reserve", "R3"),
        };
        list[0].MarkSuccess();
        list[1].MarkFailed("x");
        list[2].MarkSuccess();

        _txRepo.CountByAccountAsync(1, Arg.Any<CancellationToken>()).Returns(list.Count);
        _txRepo.ListByAccountAsync(1, 1, 2, Arg.Any<CancellationToken>()).Returns(list.Take(2).ToList());

        var h = new ListAccountTransactionsHandler(_txRepo, _accountRepo);
        var result = await h.Handle(new ListAccountTransactionsQuery(1, 1, 2), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Items.Count);
        Assert.Equal(3, result.Data.Total);
        Assert.Equal(2, result.Data.TotalPages);
    }
}
