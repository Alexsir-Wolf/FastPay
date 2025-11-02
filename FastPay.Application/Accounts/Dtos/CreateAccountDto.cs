namespace FastPay.Application.Accounts.Dtos;

public sealed class CreateAccountDto
{
    public int Id { get; init; }
    public string ClientId { get; init; }
    public decimal AvailableBalance { get; init; }
    public decimal ReservedBalance { get; init; }
    public decimal CreditLimit { get; init; }
    public string Currency { get; init; }
    public string Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

