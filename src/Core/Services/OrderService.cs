using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Import;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;
    private readonly INotificationSink _notificationSink;

    public event EventHandler<OrderChangedEventArgs>? Changed;

    public OrderService(IOrderStore store, INotificationSink notificationSink)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _notificationSink = notificationSink
            ?? throw new ArgumentNullException(nameof(notificationSink));
    }

    public Order CreateOrder(string customerId)
    {
        string id = Guid.NewGuid().ToString("N")[..8];
        Order order = Order.Create(id, customerId);
        _store.Add(order);
        OnChanged(order, OrderChangeKind.Created);
        return order;
    }

    public void AddLine(string orderId, Product product, int quantity)
    {
        Order order = GetRequired(orderId);
        order.AddLine(product, quantity);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
    }

    public void Confirm(string orderId, Customer customer)
    {
        Order order = GetRequired(orderId);
        OrderConfirmationService.Confirm(order, customer);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.Confirmed);
    }

    public void Cancel(string orderId)
    {
        Order order = GetRequired(orderId);
        order.Cancel();
        _store.Update(order);
        OnChanged(order, OrderChangeKind.Cancelled);
    }

    public IReadOnlyList<Order> All() =>
        _store.List();

    public Order? Find(string id) =>
        _store.GetById(id);

    public IReadOnlyList<Order> FindAll(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    public async Task<int> ImportAsync(
        IOrderLineImporter importer,
        string path,
        CancellationToken cancellationToken = default)
    {
        return await ImportAsync(
            importer,
            path,
            progress: null,
            cancellationToken);
    }

    public async Task<int> ImportAsync(
        IOrderLineImporter importer,
        string path,
        IProgress<int>? progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importer);

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Шлях до файлу імпорту не може бути порожнім",
                nameof(path));
        }

        IReadOnlyList<OrderLineImport> items =
            await importer.LoadAsync(path, cancellationToken);
        int added = 0;
        progress?.Report(0);

        for (int index = 0; index < items.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TryImport(items[index]))
            {
                added++;
            }

            progress?.Report((index + 1) * 100 / items.Count);
        }

        if (items.Count == 0)
        {
            progress?.Report(100);
        }

        return added;
    }

    public async Task<int> ImportStreamingAsync(
        IStreamingOrderLineImporter importer,
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importer);

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Шлях до файлу імпорту не може бути порожнім",
                nameof(path));
        }

        int added = 0;

        await foreach (OrderLineImport item in
            importer.StreamAsync(path, cancellationToken)
                .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (TryImport(item))
            {
                added++;
            }
        }

        return added;
    }

    public async Task<int> ImportWithTimeoutAsync(
        IOrderLineImporter importer,
        string path,
        TimeSpan timeout,
        CancellationToken userToken = default)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                timeout,
                "Таймаут має бути більшим за нуль");
        }

        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
            userToken,
            timeoutSource.Token);

        return await ImportAsync(importer, path, linkedSource.Token);
    }

    private bool TryImport(OrderLineImport item)
    {
        Order order = GetRequired(item.OrderId);

        bool alreadyImported = order.Lines.Any(line =>
            string.Equals(
                line.ProductId,
                item.Line.ProductId,
                StringComparison.OrdinalIgnoreCase));

        if (alreadyImported)
        {
            return false;
        }

        order.AddLine(
            item.Line.ProductId,
            item.Line.ProductName,
            item.Line.Price,
            item.Line.Quantity);
        _store.Update(order);
        OnChanged(order, OrderChangeKind.LineAdded);
        return true;
    }

    private void OnChanged(Order order, OrderChangeKind kind)
    {
        var notification = new OrderChangedEventArgs(
            order.Id,
            kind,
            order.Lines.Count,
            order.Total);

        Changed?.Invoke(this, notification);
        _notificationSink.Notify(notification);
    }

    private Order GetRequired(string id) =>
        _store.GetById(id)
        ?? throw new DomainException($"Замовлення з id={id} не знайдено");
}
