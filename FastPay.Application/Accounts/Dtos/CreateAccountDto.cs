namespace FastPay.Application.Accounts.Dtos;

public sealed class CreateAccountDto
{
    public int Id { get; init; }
    public string ClientId { get; init; }
    public decimal AvailableBalance { get; init; }
    public decimal ReservedBalance { get; init; }
    public decimal CreditLimit { get; init; }
    public string Currency { get; init; } = "BRL";
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public IReadOnlyCollection<TransactionDto> Transactions { get; init; } = new List<TransactionDto>();
}

