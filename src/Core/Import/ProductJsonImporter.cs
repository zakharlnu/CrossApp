using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        string json = File.ReadAllText(path, Encoding.UTF8);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            List<ProductDto> products =
                JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? [];

            return new ImportResult<ProductDto>(products, []);
        }
        catch (JsonException exception)
        {
            return new ImportResult<ProductDto>(
                [],
                [$"помилка JSON: {exception.Message}"]);
        }
    }
}
