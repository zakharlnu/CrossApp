using Core.Import;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public sealed class ImportTests
{
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();
        var service = new OrderService(
            new InMemoryOrderStore(),
            new TestNotificationSink());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new JsonOrderLineImporter(),
                "file-does-not-exist.json",
                cancellationSource.Token));
    }
}
