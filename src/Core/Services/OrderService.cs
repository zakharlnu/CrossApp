using Core.Abstractions;
using Core.Domain;
using Core.Events;

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
        ?? throw new InvalidOperationException($"Замовлення з id={id} не знайдено");
}
