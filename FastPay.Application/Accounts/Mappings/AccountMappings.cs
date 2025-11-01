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
            AvailableBalance = account.AvailableBalance.Amount,
            ReservedBalance = account.ReservedBalance.Amount,
            CreditLimit = account.CreditLimit.Amount,
            Currency = account.AvailableBalance.Currency,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt
        };
    }
}

