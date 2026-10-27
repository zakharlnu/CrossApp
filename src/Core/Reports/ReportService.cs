using Core.Domain;
using System.Diagnostics;

namespace Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<CustomerSummary> TopCustomersByTotal(
        IEnumerable<Order> source,
        int count) =>
        source
            .GroupBy(order => order.CustomerId, StringComparer.OrdinalIgnoreCase)
            .Select(group => new CustomerSummary(
                group.Key,
                group.Count(),
                group.Sum(order => order.Total)))
            .TopBy(
                summary => summary.TotalSpent,
                summary => summary.CustomerId,
                count)
            .ToList();

    public static IReadOnlyList<OrderReportLine> TopOrdersByTotal(
        IEnumerable<Order> source,
        int count) =>
        source
            .TopBy(order => order.Total, order => order.Id, count)
            .Select(ToReportLine)
            .ToList();

    public static IReadOnlyList<OrderReportLine> ExpensiveOrders(
        IEnumerable<Order> source,
        decimal minimumTotal) =>
        ExpensiveOrdersQuery(source, minimumTotal).ToList();

    public static IEnumerable<OrderReportLine> ExpensiveOrdersQuery(
        IEnumerable<Order> source,
        decimal minimumTotal) =>
        source
            .Where(order => order.Total > minimumTotal)
            .OrderByDescending(order => order.Total)
            .ThenBy(order => order.Id, StringComparer.Ordinal)
            .Select(ToReportLine);

    public static IReadOnlyList<StatusSummary> ByStatus(
        IEnumerable<Order> source)
    {
        var query =
            from order in source
            group order by order.Status into statusGroup
            let total = statusGroup.Sum(order => order.Total)
            orderby total descending, statusGroup.Key
            select new StatusSummary(
                statusGroup.Key.ToString(),
                statusGroup.Count(),
                total);

        return query.ToList();
    }

    public static IReadOnlyList<CustomerOrderSummary> ByCustomer(
        IEnumerable<Customer> customers,
        IEnumerable<Order> orders) =>
        customers
            .GroupJoin(
                orders,
                customer => customer.Id,
                order => order.CustomerId,
                (customer, customerOrders) => new CustomerOrderSummary(
                    customer.Id,
                    customer.Name,
                    customerOrders.Count(),
                    customerOrders.Sum(order => order.Total)),
                StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(summary => summary.TotalSpent)
            .ThenBy(summary => summary.CustomerId, StringComparer.Ordinal)
            .ToList();

    public static (int Orders, decimal Total, decimal Average, decimal Max) Totals(
        IEnumerable<Order> source)
    {
        List<Order> orders = source.ToList();

        return orders.Count == 0
            ? (0, 0m, 0m, 0m)
            : (
                orders.Count,
                orders.Sum(order => order.Total),
                orders.Average(order => order.Total),
                orders.Max(order => order.Total));
    }

    public static ReportBenchmarkResult BenchmarkMaterialization(
        IEnumerable<Order> source,
        decimal minimumTotal)
    {
        List<Order> orders = source.ToList();

        var stopwatch = Stopwatch.StartNew();
        List<OrderReportLine> withIntermediateToList = orders
            .Where(order => order.Total > minimumTotal)
            .ToList()
            .OrderByDescending(order => order.Total)
            .ThenBy(order => order.Id, StringComparer.Ordinal)
            .Select(ToReportLine)
            .ToList();
        stopwatch.Stop();
        TimeSpan withIntermediateTime = stopwatch.Elapsed;

        stopwatch.Restart();
        List<OrderReportLine> withoutIntermediateToList = orders
            .Where(order => order.Total > minimumTotal)
            .OrderByDescending(order => order.Total)
            .ThenBy(order => order.Id, StringComparer.Ordinal)
            .Select(ToReportLine)
            .ToList();
        stopwatch.Stop();

        if (withoutIntermediateToList.Count != withIntermediateToList.Count)
        {
            throw new InvalidOperationException(
                "Порівнювані запити повернули різну кількість записів");
        }

        return new ReportBenchmarkResult(
            orders.Count,
            withoutIntermediateToList.Count,
            stopwatch.Elapsed,
            withIntermediateTime);
    }

    private static OrderReportLine ToReportLine(Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.Lines.Count,
            order.Total);
}
