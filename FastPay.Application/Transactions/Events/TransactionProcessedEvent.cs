using FastPay.Application.Common.Events;
using FastPay.Domain.Enums;

namespace FastPay.Application.Transactions.Events;

public sealed record TransactionProcessedEvent(
    string ReferenceId,
    string Operation,
    TransactionStatus Status,
    int AccountId,
    int? DestinationAccountId,
    decimal Amount,
    string Currency,
    decimal AvailableBalance,
    decimal ReservedBalance,
    decimal UsedCredit,
    DateTime Timestamp,
    string? MetadataJson
) : IEvent
{
    public string EventType => nameof(TransactionProcessedEvent);
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
