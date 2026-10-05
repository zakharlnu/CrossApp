using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            List<JsonElement>? elements =
                JsonSerializer.Deserialize<List<JsonElement>>(json, options);

            if (elements is null)
            {
                return new ImportResult<ProductDto>(
                    [],
                    ["JSON має містити масив товарів"]);
            }

            var products = new List<ProductDto>();
            var errors = new List<string>();

            for (int index = 0; index < elements.Count; index++)
            {
                try
                {
                    ProductJsonRow? row = elements[index].Deserialize<ProductJsonRow>(options);

                    switch (row is null
                        ? new ParseFailed("об'єкт порожній")
                        : ParseRow(row))
                    {
                        case ParseOk ok:
                            products.Add(ok.Value);
                            break;
                        case ParseFailed failed:
                            errors.Add($"елемент {index + 1}: {failed.Reason}");
                            break;
                    }
                }
                catch (JsonException exception)
                {
                    errors.Add($"елемент {index + 1}: некоректний формат — {exception.Message}");
                }
            }

            return new ImportResult<ProductDto>(products, errors);
        }
        catch (JsonException exception)
        {
            return new ImportResult<ProductDto>(
                [],
                [$"помилка JSON: {exception.Message}"]);
        }
    }

    private static ParseOutcome ParseRow(ProductJsonRow row)
    {
        return row switch
        {
            _ when string.IsNullOrWhiteSpace(row.Id) =>
                new ParseFailed("ідентифікатор порожній"),
            _ when string.IsNullOrWhiteSpace(row.Name) =>
                new ParseFailed("назва порожня"),
            _ when string.IsNullOrWhiteSpace(row.Category) =>
                new ParseFailed("категорія порожня"),
            { Price: null } => new ParseFailed("ціна відсутня"),
            { Price: < 0 } => new ParseFailed("ціна не може бути від'ємною"),
            {
                Id: { } id,
                Name: { } name,
                Category: { } category,
                Price: { } price
            } => new ParseOk(
                new ProductDto(id, name, category, price, EmptyToNull(row.Description))),
            _ => new ParseFailed("некоректні дані товару")
        };
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record ProductJsonRow(
        string? Id,
        string? Name,
        string? Category,
        decimal? Price,
        string? Description);

    private abstract record ParseOutcome;
    private sealed record ParseOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
