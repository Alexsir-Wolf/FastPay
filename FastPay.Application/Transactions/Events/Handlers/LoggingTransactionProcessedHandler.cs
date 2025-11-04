using FastPay.Application.Common.Events;
using Microsoft.Extensions.Logging;

namespace FastPay.Application.Transactions.Events.Handlers;

public sealed class LoggingTransactionProcessedHandler : IEventHandler<TransactionProcessedEvent>
{
    private readonly ILogger<LoggingTransactionProcessedHandler> _logger;
    public LoggingTransactionProcessedHandler(ILogger<LoggingTransactionProcessedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TransactionProcessedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Evento {EventType}: Ref={Ref}, Op={Operation}, Status={Status}, Account={AccountId}, Dest={Dest}, Amount={Amount} {Currency}",
            @event.EventType, @event.ReferenceId, @event.Operation, @event.Status, @event.AccountId, @event.DestinationAccountId, @event.Amount, @event.Currency);

        return Task.CompletedTask;
    }
}

