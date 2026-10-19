using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Шлях до журналу не може бути порожнім",
                nameof(path));
        }

        _path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public void Notify(OrderChangedEventArgs notification)
    {
        string line =
            $"[FILE SINK] {notification.At:HH:mm:ss} {notification.Kind,-10} " +
            $"{notification.OrderId} позицій: {notification.LinesCount}, " +
            $"сума: {notification.Total:F2}";

        File.AppendAllText(
            _path,
            line + Environment.NewLine);
    }
}
