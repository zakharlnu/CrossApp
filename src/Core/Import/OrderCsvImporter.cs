using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class OrderCsvImporter
{
    private const char Separator = ';';

    public static OrderImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var customers = new List<CustomerDto>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int index = 0; index < lines.Length; index++)
        {
            int lineNumber = index + 1;
            string line = lines[index];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.Trim().Equals(
                    "type;id;name;value;details;note",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            switch (ParseLine(line))
            {
                case ParseProduct product:
                    products.Add(product.Value);
                    break;
                case ParseCustomer customer:
                    customers.Add(customer.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {lineNumber}: {failed.Reason}");
                    break;
            }
        }

        return new OrderImportResult(products, customers, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: not 6 } => new ParseFailed(
                $"очікую 6 колонок, отримано {parts.Length}"),
            ["P" or "C", "", _, _, _, _] => new ParseFailed("ідентифікатор порожній"),
            ["P" or "C", _, "", _, _, _] => new ParseFailed("назва порожня"),
            ["P", _, _, _, "", _] => new ParseFailed("категорія товару порожня"),
            ["P", _, _, var price, _, _] when
                !decimal.TryParse(
                    price,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal parsedPrice) || parsedPrice < 0
                => new ParseFailed($"ціна '{price}' не є невід'ємним числом"),
            ["P", var id, var name, var price, var category, var description] =>
                new ParseProduct(
                new ProductDto(
                    id,
                    name,
                    category,
                    decimal.Parse(price, NumberStyles.Number, CultureInfo.InvariantCulture),
                    EmptyToNull(description))),
            ["C", var id, var name, var email, var phone, var address] =>
                new ParseCustomer(
                new CustomerDto(
                    id,
                    name,
                    EmptyToNull(email),
                    EmptyToNull(phone),
                    EmptyToNull(address))),
            [var type, _, _, _, _, _] => new ParseFailed(
                $"невідомий тип запису '{type}'"),
            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    private static string? EmptyToNull(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private abstract record ParseOutcome;
    private sealed record ParseProduct(ProductDto Value) : ParseOutcome;
    private sealed record ParseCustomer(CustomerDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
