using FastPay.Application.Transactions.Commands;
using FastPay.Application.Transactions.Validators;
using Xunit;

namespace FastPay.Application.Tests;

public class ProcessTransactionCommandValidatorTests
{
    private readonly ProcessTransactionCommandValidator _validator = new();

    [Fact]
    public void Invalid_Operation_Fails()
    {
        var cmd = new ProcessTransactionCommand("teste", 1, 0, 100, "BRL", "REF-1", null);
        var r = _validator.Validate(cmd);
        Assert.False(r.IsValid);
    }

    [Fact]
    public void Missing_Currency_Fails()
    {
        var cmd = new ProcessTransactionCommand("credit", 1, 0, 100, "", "REF-1", null);
        var r = _validator.Validate(cmd);
        Assert.False(r.IsValid);
    }

    [Theory]
    [InlineData("BRL")]
    [InlineData("USD")]
    [InlineData("EUR")]
    public void Valid_Currency_Passes(string currency)
    {
        var cmd = new ProcessTransactionCommand("credit", 1, 0, 100, currency, "REF-1", null);
        var r = _validator.Validate(cmd);
        Assert.True(r.IsValid);
    }

    [Fact]
    public void Negative_Amount_Fails()
    {
        var cmd = new ProcessTransactionCommand("credit", 1, 0, -1, "BRL", "REF-1", null);
        var r = _validator.Validate(cmd);
        Assert.False(r.IsValid);
    }

    [Fact]
    public void Transfer_Requires_Different_Accounts()
    {
        var cmd = new ProcessTransactionCommand("transfer", 1, 1, 100, "BRL", "REF-1", null);
        var r = _validator.Validate(cmd);
        Assert.False(r.IsValid);
    }

    [Fact]
    public void Transfer_Requires_Destination()
    {
        var cmd = new ProcessTransactionCommand("transfer", 1, 0, 100, "BRL", "REF-1", null);
        var r = _validator.Validate(cmd);
        Assert.False(r.IsValid);
    }
}

