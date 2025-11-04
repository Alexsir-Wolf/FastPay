namespace FastPay.Domain.ValueObjects;

public sealed class Money
{
    public decimal Amount { get; private set; }

    private Money() 
    {
    }

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("O valor não pode ser negativo.");

        Amount = decimal.Round(amount, 2);
    }

    public static Money Zero()
    {
        return new Money(0);
    }

    public static Money FromCents(long cents)
    {
        if (cents < 0)
            throw new ArgumentException("O valor não pode ser negativo.");
        var major = cents / 100m;
        return new Money(major);
    }

    public long ToCents()
    {
        return (long)(Amount * 100m);
    }

    public Money Add(Money other)
    {
        return new Money(Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        if (Amount < other.Amount)
            throw new InvalidOperationException("Saldo insuficiente.");

        return new Money(Amount - other.Amount);
    }

    public override string ToString()
    {
        return $"{Amount:N2}";
    }
}
