using Core.Domain;

namespace Core.Tests;

internal static class TestData
{
    public static IReadOnlyList<Order> Orders()
    {
        Order orderB = CreateOrder("O-B", "C-002", 1_000m);
        Order orderSmall = CreateOrder("O-SMALL", "C-003", 100m);
        Order orderA = CreateOrder("O-A", "C-001", 1_000m);
        Order orderMiddle = CreateOrder("O-MID", "C-004", 500m);

        return [orderB, orderSmall, orderA, orderMiddle];
    }

    public static Order CreateOrder(
        string orderId = "O-TEST",
        string customerId = "C-TEST",
        decimal price = 100m)
    {
        Order order = Order.Create(orderId, customerId);
        order.AddLine("P-BASE", "Базовий товар", price, 1);
        return order;
    }
}
