using Core.Abstractions;
using Core.Events;

namespace Core.Tests;

internal sealed class TestNotificationSink : INotificationSink
{
    public List<OrderChangedEventArgs> Notifications { get; } = [];

    public void Notify(OrderChangedEventArgs eventArgs) =>
        Notifications.Add(eventArgs);
}
