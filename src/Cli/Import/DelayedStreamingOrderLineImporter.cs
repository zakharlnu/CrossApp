using System.Runtime.CompilerServices;
using Core.Abstractions;
using Core.Import;

namespace Cli.Import;

internal sealed class DelayedStreamingOrderLineImporter(
    IStreamingOrderLineImporter inner,
    TimeSpan delayPerItem) : IStreamingOrderLineImporter
{
    public async IAsyncEnumerable<OrderLineImport> StreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (OrderLineImport item in
            inner.StreamAsync(path, cancellationToken)
                .WithCancellation(cancellationToken))
        {
            await Task.Delay(delayPerItem, cancellationToken);
            yield return item;
        }
    }
}
