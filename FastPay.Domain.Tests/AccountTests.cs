using FastPay.Domain.Common;
using FastPay.Domain.Entities;
using FastPay.Domain.Enums;
using FastPay.Domain.ValueObjects;
using Xunit;

namespace FastPay.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Credit_Increases_Available_When_No_UsedCredit()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));

        account.Credit(new Money(25));

        Assert.Equal(125m, account.AvailableBalance.Amount);
        Assert.Equal(0m, account.UsedCredit.Amount);
    }

    [Fact]
    public void Credit_Reduces_UsedCredit_First()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));
        account.Debit(new Money(120));

        account.Credit(new Money(30));

        Assert.Equal(10m, account.AvailableBalance.Amount); 
        Assert.Equal(0m, account.UsedCredit.Amount);
    }

    [Fact]
    public void Debit_Uses_Available_Then_Credit()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));

        account.Debit(new Money(140));

        Assert.Equal(0m, account.AvailableBalance.Amount);
        Assert.Equal(40m, account.UsedCredit.Amount);
    }

    [Fact]
    public void Debit_Throws_When_Exceeding_Total_Available()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));

        Assert.Throws<DomainException>(() => account.Debit(new Money(200)));
    }

    [Fact]
    public void Reserve_Moves_From_Available_To_Reserved()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));

        account.Reserve(new Money(60));

        Assert.Equal(40m, account.AvailableBalance.Amount);
        Assert.Equal(60m, account.ReservedBalance.Amount);
    }

    [Fact]
    public void Reserve_Throws_When_Insufficient_Available()
    {
        var account = new Account("C-1", "BRL", new Money(50), new Money(0));
        Assert.Throws<DomainException>(() => account.Reserve(new Money(60)));
    }

    [Fact]
    public void Capture_Reduces_Reserved()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        account.Reserve(new Money(70));

        account.Capture(new Money(30));

        Assert.Equal(30m, account.AvailableBalance.Amount);
        Assert.Equal(40m, account.ReservedBalance.Amount);
    }

    [Fact]
    public void Capture_Throws_When_Insufficient_Reserved()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        account.Reserve(new Money(20));
        Assert.Throws<DomainException>(() => account.Capture(new Money(30)));
    }

    [Fact]
    public void ReverseDebit_Prefers_Reducing_UsedCredit()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));
        account.Debit(new Money(140));

        account.ReverseDebit(new Money(25));

        Assert.Equal(0m, account.AvailableBalance.Amount);
        Assert.Equal(15m, account.UsedCredit.Amount);
    }

    [Fact]
    public void ReverseDebit_Remainder_Goes_To_Available()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(50));
        account.Debit(new Money(120));

        account.ReverseDebit(new Money(50));

        Assert.Equal(30m, account.AvailableBalance.Amount);
        Assert.Equal(0m, account.UsedCredit.Amount);
    }

    [Fact]
    public void ReverseReserve_Moves_From_Reserved_To_Available()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        account.Reserve(new Money(40));

        account.ReverseReserve(new Money(15));

        Assert.Equal(75m, account.AvailableBalance.Amount);
        Assert.Equal(25m, account.ReservedBalance.Amount);
    }

    [Fact]
    public void ReverseReserve_Throws_When_Insufficient_Reserved()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        account.Reserve(new Money(10));
        Assert.Throws<DomainException>(() => account.ReverseReserve(new Money(20)));
    }

    [Fact]
    public void ChangeStatus_Updates_Status()
    {
        var account = new Account("C-1", "BRL", new Money(100), new Money(0));
        account.ChangeStatus(AccountStatus.Blocked);
        Assert.Equal(AccountStatus.Blocked, account.Status);
    }
}
