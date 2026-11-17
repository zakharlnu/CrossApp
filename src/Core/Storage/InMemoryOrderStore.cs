using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<string, Order> _orders;
    private readonly object _sync = new();

    public InMemoryOrderStore(IEnumerable<Order>? seed = null)
    {
        _orders = (seed ?? []).ToDictionary(
            order => order.Id,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Order> List()
    {
        lock (_sync)
        {
            return _orders.Values.ToList();
        }
    }

    public Order? GetById(string id)
    {
        lock (_sync)
        {
            return _orders.GetValueOrDefault(id);
        }
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        lock (_sync)
        {
            if (!_orders.TryAdd(order.Id, order))
            {
                throw new DomainException(
                    $"Замовлення з id={order.Id} уже існує");
            }
        }
    }

    public void Update(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        lock (_sync)
        {
            _orders[order.Id] = order;
        }
    }

    public bool Remove(string id)
    {
        lock (_sync)
        {
            return _orders.Remove(id);
        }
    }
}
