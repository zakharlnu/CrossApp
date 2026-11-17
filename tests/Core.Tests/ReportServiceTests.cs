using Core.Reports;

namespace Core.Tests;

public sealed class ReportServiceTests
{
    [Theory]
    [InlineData(600, 2)]
    [InlineData(100, 3)]
    [InlineData(1000, 0)]
    public void ExpensiveOrders_CountsOnlyAboveThreshold(
        int threshold,
        int expected)
    {
        IReadOnlyList<OrderReportLine> result =
            ReportService.ExpensiveOrders(TestData.Orders(), threshold);

        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopOrdersByTotal_UsesIdAsDeterministicSecondKey()
    {
        IReadOnlyList<string> ids = ReportService
            .TopOrdersByTotal(TestData.Orders(), 2)
            .Select(line => line.OrderId)
            .ToList();

        Assert.Equal(["O-A", "O-B"], ids);
    }

    [Fact]
    public void Totals_WithEmptySource_ReturnsZeros()
    {
        var totals = ReportService.Totals([]);

        Assert.Equal(0, totals.Orders);
        Assert.Equal(0m, totals.Total);
        Assert.Equal(0m, totals.Average);
        Assert.Equal(0m, totals.Max);
    }
}
