using FastPay.Application.Accounts.Dtos;
using FastPay.Domain.Entities;

namespace FastPay.Application.Accounts.Mappings;

public static class AccountMappings
{
    public static AccountDto ToAccountDto(this Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            ClientId = account.ClientId,
            Currency = account.Currency,
            AvailableBalance = account.AvailableBalance.Amount,
            ReservedBalance = account.ReservedBalance.Amount,
            CreditLimit = account.CreditLimit.Amount,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt,
            Transactions = account.Transactions
                .OrderByDescending(t => t.Timestamp)
                .Select(t => t.ToTransactionDto())
                .ToList()
        };
    }

    public static CreateAccountDto ToCreateAccountDto(this Account account)
    {
        return new CreateAccountDto
        {
            Id = account.Id,
            ClientId = account.ClientId,
            AvailableBalance = account.AvailableBalance.Amount,
            ReservedBalance = account.ReservedBalance.Amount,
            CreditLimit = account.CreditLimit.Amount,
            Currency = account.Currency,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt
        };
    }
}

