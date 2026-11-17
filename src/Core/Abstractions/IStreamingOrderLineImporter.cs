using Core.Import;

namespace Core.Abstractions;

public interface IStreamingOrderLineImporter
{
    IAsyncEnumerable<OrderLineImport> StreamAsync(
        string path,
        CancellationToken cancellationToken = default);
}
