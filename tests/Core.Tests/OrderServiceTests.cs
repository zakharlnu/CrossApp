using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public sealed class OrderServiceTests
{
    [Fact]
    public void AddLine_WithNegativeQuantity_Throws_AndKeepsState()
    {
        Order order = TestData.CreateOrder();
        var store = new InMemoryOrderStore([order]);
        var service = new OrderService(store, new TestNotificationSink());
        Product product = Product.Create("P-NEW", "Новий товар", "Тести", 50m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.AddLine(order.Id, product, -1));

        Order unchanged = Assert.IsType<Order>(store.GetById(order.Id));
        Assert.Single(unchanged.Lines);
        Assert.Equal(100m, unchanged.Total);
    }

    [Fact]
    public void AddLine_ToConfirmedOrder_ThrowsDomainException_AndKeepsState()
    {
        Order order = TestData.CreateOrder();
        order.Confirm();
        var store = new InMemoryOrderStore([order]);
        var service = new OrderService(store, new TestNotificationSink());
        Product product = Product.Create("P-NEW", "Новий товар", "Тести", 50m);

        DomainException exception = Assert.Throws<DomainException>(
            () => service.AddLine(order.Id, product, 1));

        Assert.Contains("Confirmed", exception.Message);
        Assert.Single(order.Lines);
        Assert.Equal(100m, order.Total);
    }

    [Fact]
    public void AddLine_RaisesChangedExactlyOnce()
    {
        Order order = TestData.CreateOrder();
        var service = new OrderService(
            new InMemoryOrderStore([order]),
            new TestNotificationSink());
        Product product = Product.Create("P-NEW", "Новий товар", "Тести", 50m);
        var events = new List<OrderChangedEventArgs>();
        service.Changed += (_, eventArgs) => events.Add(eventArgs);

        service.AddLine(order.Id, product, 2);

        OrderChangedEventArgs changed = Assert.Single(events);
        Assert.Equal(OrderChangeKind.LineAdded, changed.Kind);
        Assert.Equal(order.Id, changed.OrderId);
        Assert.Equal(200m, order.Total);
    }

    [Fact]
    public async Task ImportAsync_SkipsExistingProduct_AndRaisesEventForNewLine()
    {
        Order order = TestData.CreateOrder();
        var service = new OrderService(
            new InMemoryOrderStore([order]),
            new TestNotificationSink());
        IReadOnlyList<OrderLineImport> items =
        [
            OrderLineImport.FromDto(new OrderLineImportDto(
                order.Id, "P-BASE", "Базовий товар", 100m, 1)),
            OrderLineImport.FromDto(new OrderLineImportDto(
                order.Id, "P-NEW", "Новий товар", 25m, 2))
        ];
        var events = new List<OrderChangedEventArgs>();
        service.Changed += (_, eventArgs) => events.Add(eventArgs);

        int added = await service.ImportAsync(
            new StubOrderLineImporter(items),
            "unused.json");

        Assert.Equal(1, added);
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(150m, order.Total);
        Assert.Single(events);
    }
}
