using FastPay.Domain.Common;
using FastPay.Domain.Enums;
using FastPay.Domain.ValueObjects;

namespace FastPay.Domain.Entities;

public class Account : Entity<int>
{
    private Account() 
    {
    }
    public Account(string clientId, string currency, Money initialBalance, Money creditLimit)
    {
        ClientId = clientId;
        Currency = currency.ToUpperInvariant();
        AvailableBalance = initialBalance ?? Money.Zero();
        ReservedBalance = Money.Zero();
        CreditLimit = creditLimit ?? Money.Zero();
        UsedCredit = Money.Zero();
        Status = AccountStatus.Active;
    }

    public string ClientId { get; private set; }
    public string Currency { get; private set; }
    public Money AvailableBalance { get; private set; }
    public Money ReservedBalance { get; private set; }
    public Money CreditLimit { get; private set; }
    public Money UsedCredit { get; private set; }
    public AccountStatus Status { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    private readonly List<Transaction> _transactions = new(); 

    public void ChangeStatus(AccountStatus status)
    {
        Status = status;
        MarkUpdated();
    }

    public void Credit(Money amount)
    {
        if (amount is null) throw new ArgumentNullException(nameof(amount));

        if (UsedCredit.Amount > 0)
        {
            var toReduce = Math.Min(UsedCredit.Amount, amount.Amount);
            UsedCredit = new Money(UsedCredit.Amount - toReduce);
            var remaining = amount.Amount - toReduce;
            if (remaining > 0)
            {
                AvailableBalance = new Money(AvailableBalance.Amount + remaining);
            }
        }
        else
        {
            AvailableBalance = new Money(AvailableBalance.Amount + amount.Amount);
        }

        MarkUpdated();
    }

    public void Debit(Money amount)
    {
        if (amount is null) throw new ArgumentNullException(nameof(amount));

        var availableCredit = CreditLimit.Amount - UsedCredit.Amount;
        var totalAvailable = AvailableBalance.Amount + Math.Max(0, availableCredit);
        if (amount.Amount > totalAvailable)
            throw new DomainException("Saldo insuficiente considerando o limite de crédito.");

        var fromAvailable = Math.Min(AvailableBalance.Amount, amount.Amount);
        if (fromAvailable > 0)
            AvailableBalance = new Money(AvailableBalance.Amount - fromAvailable);

        var remaining = amount.Amount - fromAvailable;
        if (remaining > 0)
            UsedCredit = new Money(UsedCredit.Amount + remaining);

        MarkUpdated();
    }

    public void Reserve(Money amount)
    {
        if (amount is null) throw new ArgumentNullException(nameof(amount));
        if (amount.Amount > AvailableBalance.Amount)
            throw new DomainException("Saldo disponível insuficiente para reserva.");

        AvailableBalance = new Money(AvailableBalance.Amount - amount.Amount);
        ReservedBalance = new Money(ReservedBalance.Amount + amount.Amount);
        MarkUpdated();
    }

    public void Capture(Money amount)
    {
        if (amount is null) throw new ArgumentNullException(nameof(amount));
        if (amount.Amount > ReservedBalance.Amount)
            throw new DomainException("Saldo reservado insuficiente para captura.");

        ReservedBalance = new Money(ReservedBalance.Amount - amount.Amount);
        MarkUpdated();
    }

    public void ReverseDebit(Money amount)
    {
        if (amount is null) throw new ArgumentNullException(nameof(amount));

        if (UsedCredit.Amount > 0)
        {
            var toReduce = Math.Min(UsedCredit.Amount, amount.Amount);
            UsedCredit = new Money(UsedCredit.Amount - toReduce);
            var remaining = amount.Amount - toReduce;
            if (remaining > 0)
                AvailableBalance = new Money(AvailableBalance.Amount + remaining);
        }
        else
        {
            AvailableBalance = new Money(AvailableBalance.Amount + amount.Amount);
        }

        MarkUpdated();
    }

    public void ReverseReserve(Money amount)
    {
        if (amount is null) throw new ArgumentNullException(nameof(amount));
        if (amount.Amount > ReservedBalance.Amount)
            throw new DomainException("Não há reserva suficiente para estorno.");

        ReservedBalance = new Money(ReservedBalance.Amount - amount.Amount);
        AvailableBalance = new Money(AvailableBalance.Amount + amount.Amount);
        MarkUpdated();
    }
}
