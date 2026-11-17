using Core.Domain;

namespace Core.Abstractions;

public interface IOrderStore
{
    IReadOnlyList<Order> List();
    Order? GetById(string id);
    void Add(Order order);
    void Update(Order order);
    bool Remove(string id);

    ValueTask<IReadOnlyList<Order>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(List());
    }

    ValueTask<Order?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(GetById(id));
    }

    ValueTask AddAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Add(order);
        return ValueTask.CompletedTask;
    }

    ValueTask UpdateAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Update(order);
        return ValueTask.CompletedTask;
    }

    ValueTask<bool> RemoveAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(Remove(id));
    }
}
