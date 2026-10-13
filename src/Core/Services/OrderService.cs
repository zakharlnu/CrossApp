using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class OrderService
{
    private readonly IOrderStore _store;

    public OrderService(IOrderStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Order CreateOrder(string customerId)
    {
        string id = Guid.NewGuid().ToString("N")[..8];
        Order order = Order.Create(id, customerId);
        _store.Add(order);
        return order;
    }

    public void AddLine(string orderId, Product product, int quantity)
    {
        Order order = GetRequired(orderId);
        order.AddLine(product, quantity);
        _store.Update(order);
    }

    public void Confirm(string orderId, Customer customer)
    {
        Order order = GetRequired(orderId);
        OrderConfirmationService.Confirm(order, customer);
        _store.Update(order);
    }

    public IReadOnlyList<Order> All() =>
        _store.List();

    public Order? Find(string id) =>
        _store.GetById(id);

    private Order GetRequired(string id) =>
        _store.GetById(id)
        ?? throw new InvalidOperationException($"Замовлення з id={id} не знайдено");
}
