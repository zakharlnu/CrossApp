namespace Infrastructure.Persistence;

public sealed class OrderRow
{
    public string Id { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public string Status { get; set; } = "";
    public List<OrderLineRow> Lines { get; set; } = [];
}

public sealed class OrderLineRow
{
    public int Id { get; set; }
    public string OrderId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public OrderRow Order { get; set; } = null!;
}
