using Core.Dto;

namespace Core.Domain;

public sealed class Product
{
    public string Id { get; }
    public string Name { get; }
    public string Category { get; }
    public decimal Price { get; private set; }
    public string? Description { get; private set; }

    private Product(
        string id,
        string name,
        string category,
        decimal price,
        string? description)
    {
        Id = id;
        Name = name;
        Category = category;
        Price = price;
        Description = description;
    }

    public static Product Create(
        string id,
        string name,
        string category,
        decimal price,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Ідентифікатор товару не може бути порожнім",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Назва товару не може бути порожньою",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException(
                "Категорія товару не може бути порожньою",
                nameof(category));
        }

        ValidatePrice(price);

        return new Product(
            id.Trim(),
            name.Trim(),
            category.Trim(),
            price,
            NormalizeOptional(description));
    }

    public void ChangePrice(decimal newPrice)
    {
        ValidatePrice(newPrice);
        Price = newPrice;
    }

    public void UpdateDescription(string? description)
    {
        Description = NormalizeOptional(description);
    }

    public ProductDto ToDto() =>
        new(Id, Name, Category, Price, Description);

    public static Product FromDto(ProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return Create(dto.Id, dto.Name, dto.Category, dto.Price, dto.Description);
    }

    public override string ToString() =>
        $"{Id} {Name} [{Category}] — {Price:F2}";

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                price,
                "Ціна товару не може бути від'ємною");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
