using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Dto;
using Core.Import;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public sealed class AdditionalAsyncTests
{
    [Fact]
    public async Task ImportStreamingAsync_AddsLinesFromAsyncSequence()
    {
        var order = TestData.CreateOrder();
        var service = new OrderService(
            new InMemoryOrderStore([order]),
            new TestNotificationSink());
        IReadOnlyList<OrderLineImport> items =
        [
            CreateImport(order.Id, "P-STREAM-1"),
            CreateImport(order.Id, "P-STREAM-2")
        ];

        int added = await service.ImportStreamingAsync(
            new StubStreamingImporter(items),
            "unused.json");

        Assert.Equal(2, added);
        Assert.Equal(3, order.Lines.Count);
    }

    [Fact]
    public async Task ImportWithTimeoutAsync_CancelsFromTimeoutToken()
    {
        using var userSource = new CancellationTokenSource();
        var importer = new DelayedImporter();
        var service = new OrderService(
            new InMemoryOrderStore(),
            new TestNotificationSink());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportWithTimeoutAsync(
                importer,
                "unused.json",
                TimeSpan.FromMilliseconds(50),
                userSource.Token));

        Assert.False(userSource.IsCancellationRequested);
        Assert.True(importer.ReceivedToken.IsCancellationRequested);
    }

    [Fact]
    public async Task ImportAsync_ReportsProgressInPercent()
    {
        var order = TestData.CreateOrder();
        var service = new OrderService(
            new InMemoryOrderStore([order]),
            new TestNotificationSink());
        IReadOnlyList<OrderLineImport> items =
        [
            CreateImport(order.Id, "P-PROGRESS-1"),
            CreateImport(order.Id, "P-PROGRESS-2")
        ];
        var progress = new TestProgress();

        int added = await service.ImportAsync(
            new StubOrderLineImporter(items),
            "unused.json",
            progress);

        Assert.Equal(2, added);
        Assert.Equal([0, 50, 100], progress.Values);
    }

    private static OrderLineImport CreateImport(string orderId, string productId) =>
        OrderLineImport.FromDto(new OrderLineImportDto(
            orderId,
            productId,
            "Тестовий товар",
            10m,
            1));

    private sealed class StubStreamingImporter(
        IReadOnlyList<OrderLineImport> items) : IStreamingOrderLineImporter
    {
        public async IAsyncEnumerable<OrderLineImport> StreamAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (OrderLineImport item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                yield return item;
            }
        }
    }

    private sealed class DelayedImporter : IOrderLineImporter
    {
        public CancellationToken ReceivedToken { get; private set; }

        public async Task<IReadOnlyList<OrderLineImport>> LoadAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            ReceivedToken = cancellationToken;
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return [];
        }
    }

    private sealed class TestProgress : IProgress<int>
    {
        public List<int> Values { get; } = [];

        public void Report(int value) => Values.Add(value);
    }
}
