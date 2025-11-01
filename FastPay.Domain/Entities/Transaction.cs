using FastPay.Domain.Common;

namespace FastPay.Domain.Entities;

public class Transaction : Entity<int>
{
    private Transaction() 
    {
    }

    public Transaction(int accountId, decimal amount)
    {
        AccountId = accountId;
        Amount = amount;
        Timestamp = DateTime.UtcNow;
    }

    public int AccountId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime Timestamp { get; private set; }    
}
