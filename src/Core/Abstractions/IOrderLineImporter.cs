using Core.Import;

namespace Core.Abstractions;

public interface IOrderLineImporter
{
    Task<IReadOnlyList<OrderLineImport>> LoadAsync(
        string path,
        CancellationToken cancellationToken = default);
}
