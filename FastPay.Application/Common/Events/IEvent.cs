namespace FastPay.Application.Common.Events;

public interface IEvent
{
    string EventType { get; }
    DateTime OccurredAt { get; }
}

