namespace Api.Contracts;

public sealed record SaveOrderRequest(
    string? CustomerId,
    IReadOnlyList<SaveOrderLineRequest>? Lines)
{
    public bool TryValidate(out Dictionary<string, string[]> errors)
    {
        var found = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(CustomerId))
        {
            found["customerId"] = ["Ідентифікатор клієнта обов'язковий."];
        }
        else if (CustomerId.Length > 100)
        {
            found["customerId"] =
                ["Ідентифікатор клієнта має містити не більше 100 символів."];
        }

        if (Lines is null || Lines.Count == 0)
        {
            found["lines"] = ["Замовлення повинно містити хоча б один рядок."];
        }
        else
        {
            for (int index = 0; index < Lines.Count; index++)
            {
                Lines[index].Validate(index, found);
            }
        }

        errors = found;
        return found.Count == 0;
    }
}

public sealed record SaveOrderLineRequest(
    string? ProductId,
    string? ProductName,
    decimal Price,
    int Quantity)
{
    internal void Validate(int index, Dictionary<string, string[]> errors)
    {
        string prefix = $"lines[{index}]";

        if (string.IsNullOrWhiteSpace(ProductId))
        {
            errors[$"{prefix}.productId"] = ["Ідентифікатор товару обов'язковий."];
        }
        else if (ProductId.Length > 100)
        {
            errors[$"{prefix}.productId"] =
                ["Ідентифікатор товару має містити не більше 100 символів."];
        }

        if (string.IsNullOrWhiteSpace(ProductName))
        {
            errors[$"{prefix}.productName"] = ["Назва товару обов'язкова."];
        }
        else if (ProductName.Length > 200)
        {
            errors[$"{prefix}.productName"] =
                ["Назва товару має містити не більше 200 символів."];
        }

        if (Price < 0)
        {
            errors[$"{prefix}.price"] = ["Ціна не може бути від'ємною."];
        }

        if (Quantity <= 0)
        {
            errors[$"{prefix}.quantity"] = ["Кількість має бути більшою за нуль."];
        }
    }
}
