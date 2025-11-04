using FastPay.Application.Accounts.Dtos;
using FastPay.Domain.Entities;

namespace FastPay.Application.Accounts.Mappings;

public static class TransactionMappings
{
    public static TransactionDto ToTransactionDto(this Transaction t)
    {
        return new TransactionDto
        {
            Id = t.Id,
            AccountId = t.AccountId,
            DestinationAccountId = t.DestinationAccountId,
            Amount = t.Amount,
            Currency = t.Currency,
            Operation = t.Operation,
            Status = t.Status,
            ReferenceId = t.ReferenceId,
            ErrorMessage = t.ErrorMessage,
            Timestamp = t.Timestamp
        };
    }
}

