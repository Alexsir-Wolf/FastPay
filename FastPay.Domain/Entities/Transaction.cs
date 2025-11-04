using FastPay.Domain.Common;
using FastPay.Domain.Enums;

namespace FastPay.Domain.Entities;

public class Transaction : Entity<int>
{
    private Transaction() 
    {
    }

    public Transaction(
        int accountId,
        decimal amount,
        string currency,
        string operation,
        string referenceId,
        int? destinationAccountId = null,
        string? metadataJson = null)
    {
        AccountId = accountId;
        Amount = amount;
        Currency = currency.ToUpperInvariant();
        Operation = operation.ToLowerInvariant();
        ReferenceId = referenceId;
        DestinationAccountId = destinationAccountId;
        MetadataJson = metadataJson;
        Status = TransactionStatus.Pending;
        Timestamp = DateTime.UtcNow;
    }

    public int AccountId { get; private set; }
    public int? DestinationAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string Operation { get; private set; } = string.Empty;
    public TransactionStatus Status { get; private set; }
    public string ReferenceId { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTime Timestamp { get; private set; }    

    public void MarkSuccess()
    {
        Status = TransactionStatus.Success;
        MarkUpdated();
    }

    public void MarkFailed(string error)
    {
        Status = TransactionStatus.Failed;
        ErrorMessage = error;
        MarkUpdated();
    }
}
