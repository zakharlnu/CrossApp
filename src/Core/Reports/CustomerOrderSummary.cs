namespace Core.Reports;

public sealed record CustomerOrderSummary(
    string CustomerId,
    string CustomerName,
    int OrdersCount,
    decimal TotalSpent);
