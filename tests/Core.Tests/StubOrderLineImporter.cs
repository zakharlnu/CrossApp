using Core.Abstractions;
using Core.Import;

namespace Core.Tests;

internal sealed class StubOrderLineImporter(
    IReadOnlyList<OrderLineImport> items) : IOrderLineImporter
{
    public Task<IReadOnlyList<OrderLineImport>> LoadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(items);
    }
}
