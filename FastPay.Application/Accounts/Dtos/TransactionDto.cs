using FastPay.Domain.Enums;

namespace FastPay.Application.Accounts.Dtos;

public sealed class TransactionDto
{
    public int Id { get; init; }
    public int AccountId { get; init; }
    public int? DestinationAccountId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
    public TransactionStatus Status { get; init; }
    public string ReferenceId { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public DateTime Timestamp { get; init; }
}
