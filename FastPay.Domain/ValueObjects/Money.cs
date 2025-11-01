using FastPay.Domain.Constants;

namespace FastPay.Domain.ValueObjects;

public sealed class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money() 
    {
    }

    public Money(decimal amount, string currency = CurrencyCodes.BRL)
    {
        if (amount < 0)
            throw new ArgumentException("O valor não pode ser negativo.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("A moeda é obrigatória.");

        Amount = decimal.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = CurrencyCodes.BRL)
    {
        return new Money(0, currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        if (Amount < other.Amount)
            throw new InvalidOperationException("Saldo insuficiente.");

        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!Currency.Equals(other.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("As moedas devem ser iguais para realizar a operação.");
    }

    public override string ToString()
    {
        return $"{Currency} {Amount:N2}";
    }
}