using FastPay.Domain.Common;
using FastPay.Domain.Enums;
using FastPay.Domain.ValueObjects;

namespace FastPay.Domain.Entities;

public class Account : Entity<int>
{
    public int ClientId { get; private set; }
    public Money AvailableBalance { get; private set; }
    public Money ReservedBalance { get; private set; }
    public Money CreditLimit { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Account() { }

    public Account(int clientId, Money initialBalance, Money creditLimit)
    {
        ClientId = clientId;
        AvailableBalance = initialBalance ?? Money.Zero();
        ReservedBalance = Money.Zero();
        CreditLimit = creditLimit ?? Money.Zero();
        Status = AccountStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }
}
