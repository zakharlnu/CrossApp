using Core.Dto;

namespace Core.Domain;

public sealed class OrderLine
{
    public string ProductId { get; }
    public string ProductName { get; }
    public decimal Price { get; }
    public int Quantity { get; }
    public decimal Total => Price * Quantity;

    private OrderLine(string productId, string productName, decimal price, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    public static OrderLine Create(
        string productId,
        string productName,
        decimal price,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException(
                "Ідентифікатор товару не може бути порожнім",
                nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException(
                "Назва товару не може бути порожньою",
                nameof(productName));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                price,
                "Ціна товару не може бути від'ємною");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "Кількість у рядку має бути більшою за нуль");
        }

        return new OrderLine(
            productId.Trim(),
            productName.Trim(),
            price,
            quantity);
    }

    public OrderLineDto ToDto() =>
        new(ProductId, ProductName, Price, Quantity);

    public static OrderLine FromDto(OrderLineDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return Create(dto.ProductId, dto.ProductName, dto.Price, dto.Quantity);
    }

    public override string ToString() =>
        $"{ProductName}: {Quantity} × {Price:F2} = {Total:F2}";
}
