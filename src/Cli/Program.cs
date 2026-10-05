using Core.Dto;
using Core.Import;

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

OrderImportResult? result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => OrderCsvImporter.Load(path),
    ".json" => FromProducts(ProductJsonImporter.Load(path)),
    _ => null
};

if (result is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу: {Path.GetExtension(path)}");
    return 1;
}

int importedCount = result.Products.Count + result.Customers.Count;
Console.WriteLine($"Завантажено записів: {importedCount}");

foreach (ProductDto product in result.Products.Take(5))
{
    Console.WriteLine(
        $" {product.Id,-6} товар   {product.Name,-25} " +
        $"{product.Category,-15} {product.Price,10:F2}");
}

foreach (CustomerDto customer in result.Customers.Take(5))
{
    Console.WriteLine(
        $" {customer.Id,-6} клієнт  {customer.Name,-25} " +
        $"{customer.Email ?? "email не вказано"}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено записів: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($" ! {error}");
    }
}

int totalCount = importedCount + result.Errors.Count;
double errorPercentage = totalCount == 0
    ? 0
    : result.Errors.Count * 100.0 / totalCount;

Console.WriteLine(
    $"Статистика: усього {totalCount}, прийнято {importedCount}, " +
    $"пропущено {result.Errors.Count}, помилок {errorPercentage:F1}%");
Console.WriteLine(new string('-', 52));

return 0;

static OrderImportResult FromProducts(ImportResult<ProductDto> result) =>
    new(result.Items, [], result.Errors);
