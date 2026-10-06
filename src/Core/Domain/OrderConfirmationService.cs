namespace Core.Domain;

public static class OrderConfirmationService
{
    public static void Confirm(Order order, Customer customer)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(customer);

        if (!string.Equals(order.CustomerId, customer.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Замовлення {order.Id} належить клієнту {order.CustomerId}, " +
                $"а не {customer.Id}");
        }

        order.Confirm();
    }
}
