using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfOrderStore(AppDbContext db) : IOrderStore
{
    private static Order ToDomain(OrderRow row) => Order.FromDto(new OrderDto(
        row.Id, row.CustomerId, row.Status,
        row.Lines.OrderBy(x => x.Id)
            .Select(x => new OrderLineDto(x.ProductId, x.ProductName, x.Price, x.Quantity))
            .ToList(), row.CreatedAt));

    private static OrderRow ToRow(Order order) => new()
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        CreatedAt = order.CreatedAt,
        Status = order.Status.ToString(),
        Lines = order.Lines.Select(x => new OrderLineRow
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Price = x.Price,
            Quantity = x.Quantity
        }).ToList()
    };

    public IReadOnlyList<Order> List() => db.Orders.AsNoTracking()
        .Include(x => x.Lines).AsEnumerable().Select(ToDomain).ToList();

    public Order? GetById(string id) => db.Orders.AsNoTracking()
        .Include(x => x.Lines).SingleOrDefault(x => x.Id == id) is { } row
            ? ToDomain(row) : null;

    public void Add(Order order)
    {
        db.Orders.Add(ToRow(order));
        db.SaveChanges();
    }

    public void Update(Order order)
    {
        var row = db.Orders.Include(x => x.Lines).Single(x => x.Id == order.Id);
        db.OrderLines.RemoveRange(row.Lines);
        row.CustomerId = order.CustomerId;
        row.Status = order.Status.ToString();
        row.Lines = ToRow(order).Lines;
        db.SaveChanges();
    }

    public bool Remove(string id)
    {
        var row = db.Orders.SingleOrDefault(x => x.Id == id);
        if (row is null) return false;
        db.Orders.Remove(row);
        db.SaveChanges();
        return true;
    }
}
