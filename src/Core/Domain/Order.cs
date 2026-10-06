using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public bool IsConfirmed => Status == OrderStatus.Confirmed;
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(line => line.Total);

    private Order(string id, string customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
    }

    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор замовлення не може бути порожнім",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException(
                "Ідентифікатор клієнта не може бути порожнім",
                nameof(customerId));
        }

        return new Order(id.Trim(), customerId.Trim());
    }

    public void AddLine(string productId, string productName, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Замовлення {Id} має статус {Status}, рядки додавати не можна");
        }

        _lines.Add(OrderLine.Create(productId, productName, price, quantity));
    }

    public void AddLine(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        AddLine(product.Id, product.Name, product.Price, quantity);
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
        {
            throw new InvalidOperationException(
                $"Неможливо підтвердити порожнє замовлення {Id}");
        }

        ChangeStatus(OrderStatus.Confirmed);
    }

    public void Cancel()
    {
        ChangeStatus(OrderStatus.Cancelled);
    }

    public OrderDto ToDto() =>
        new(Id, CustomerId, Status.ToString(), _lines.Select(line => line.ToDto()).ToList());

    public static Order FromDto(OrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Lines is null)
        {
            throw new ArgumentException("Список рядків замовлення відсутній", nameof(dto));
        }

        Order order = Create(dto.Id, dto.CustomerId);

        foreach (OrderLineDto line in dto.Lines)
        {
            order.AddLine(line.ProductId, line.ProductName, line.Price, line.Quantity);
        }

        if (!Enum.TryParse(dto.Status, ignoreCase: true, out OrderStatus status))
        {
            throw new ArgumentException(
                $"Невідомий статус замовлення '{dto.Status}'",
                nameof(dto));
        }

        switch (status)
        {
            case OrderStatus.Draft:
                break;
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
        }

        return order;
    }

    public override string ToString() =>
        $"Замовлення {Id} для {CustomerId}: {Lines.Count} поз., сума {Total:F2}, " +
        $"статус: {Status}";

    private void ChangeStatus(OrderStatus newStatus)
    {
        bool transitionAllowed = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed or OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (!transitionAllowed)
        {
            throw new InvalidOperationException(
                $"Перехід замовлення {Id} зі статусу {Status} у {newStatus} неможливий");
        }

        Status = newStatus;
    }
}
