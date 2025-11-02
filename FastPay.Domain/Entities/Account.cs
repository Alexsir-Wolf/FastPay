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
        Status = AccountStatus.Active;
    }

    public string ClientId { get; private set; }
    public string Currency { get; private set; }
    public Money AvailableBalance { get; private set; }
    public Money ReservedBalance { get; private set; }
    public Money CreditLimit { get; private set; }
    public AccountStatus Status { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    private readonly List<Transaction> _transactions = new(); 
}
