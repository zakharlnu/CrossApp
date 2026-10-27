namespace Core.Reports;

public sealed record StatusSummary(
    string Status,
    int OrdersCount,
    decimal TotalAmount);
