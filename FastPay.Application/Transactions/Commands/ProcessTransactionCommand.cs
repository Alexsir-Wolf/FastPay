using FastPay.Application.Common;
using FastPay.Application.Transactions.Dtos;
using MediatR;

namespace FastPay.Application.Transactions.Commands;

public record ProcessTransactionCommand(
    string Operation,
    int SourceAccountId,
    int DestinationAccountId,
    long Amount,
    string Currency,
    string ReferenceId,
    IDictionary<string, object>? Metadata
    ) : IRequest<CommandResult<ProcessTransactionResponse>>
{ 
}