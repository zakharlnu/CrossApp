using Core.Domain;

namespace Api.Contracts;

public static class OrderMappings
{
    public static OrderDto ToApiDto(this Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.CreatedAt,
            order.Status.ToString(),
            order.Lines
                .Select(line => new OrderLineDto(
                    line.ProductId,
                    line.ProductName,
                    line.Price,
                    line.Quantity,
                    line.Total))
                .ToList(),
            order.Total);

    public static Order ToOrder(
        this SaveOrderRequest request,
        string id,
        DateTimeOffset? createdAt = null)
    {
        Order order = Order.Create(id, request.CustomerId!, createdAt);

        foreach (SaveOrderLineRequest line in request.Lines!)
        {
            order.AddLine(
                line.ProductId!,
                line.ProductName!,
                line.Price,
                line.Quantity);
        }

        return order;
    }
}
