using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Dto;

namespace Core.Import;

public sealed class JsonOrderLineImporter :
    IOrderLineImporter,
    IStreamingOrderLineImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<OrderLineImport>> LoadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        List<OrderLineImportDto>? dtos =
            await JsonSerializer.DeserializeAsync<List<OrderLineImportDto>>(
                stream,
                Options,
                cancellationToken);

        if (dtos is null)
        {
            throw new DomainException(
                $"Файл {Path.GetFileName(path)} не містить списку рядків замовлень");
        }

        var result = new List<OrderLineImport>(dtos.Count);

        for (int index = 0; index < dtos.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                result.Add(OrderLineImport.FromDto(dtos[index]));
            }
            catch (ArgumentException exception)
            {
                throw new DomainException(
                    $"Запис {index + 1}: {exception.Message}",
                    exception);
            }
        }

        return result;
    }

    public async IAsyncEnumerable<OrderLineImport> StreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        int index = 0;

        await foreach (OrderLineImportDto? dto in
            JsonSerializer.DeserializeAsyncEnumerable<OrderLineImportDto>(
                stream,
                Options,
                cancellationToken)
            .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            index++;

            OrderLineImport item;

            try
            {
                item = OrderLineImport.FromDto(
                    dto ?? throw new ArgumentException("Запис не може бути null"));
            }
            catch (ArgumentException exception)
            {
                throw new DomainException(
                    $"Запис {index}: {exception.Message}",
                    exception);
            }

            yield return item;
        }
    }
}
