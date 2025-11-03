using FastPay.Application.Common;
using FastPay.Application.Transactions.Dtos;
using FastPay.Domain.Enums;
using MediatR;

namespace FastPay.Application.Transactions.Commands;

public record ProcessTransactionCommand(
    TransactionOperation Operation,
    int AccountId,
    int SourceAccountId,
    int DestinationAccountId,
    decimal Amount,
    string Currency,
    string ReferenceId,
    IDictionary<string, object>? Metadata
    ) : IRequest<CommandResult<ProcessTransactionResponse>>
{ 
}

