using System.Text.Json.Serialization;
using FastPay.Domain.Enums;

namespace FastPay.Application.Transactions.Dtos;

public sealed class ProcessTransactionResponse
{
    public string TransactionId { get; init; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionStatus Status { get; init; }
    public long Balance { get; init; }
    public long ReservedBalance { get; init; }
    public long AvailableBalance { get; init; }
    public DateTime Timestamp { get; init; }
    public string? ErrorMessage { get; init; }
}
