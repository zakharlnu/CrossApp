using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs notification)
    {
        Console.WriteLine(
            $"[CONSOLE SINK] {notification.At:HH:mm:ss} {notification.Kind,-10} " +
            $"{notification.OrderId} позицій: {notification.LinesCount}, " +
            $"сума: {notification.Total:F2}");
    }
}
