using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class ProductDomainMapper
{
    public static ImportResult<Product> Map(ImportResult<ProductDto> importResult)
    {
        ArgumentNullException.ThrowIfNull(importResult);

        var products = new List<Product>();
        var errors = new List<string>(importResult.Errors);

        for (int index = 0; index < importResult.Items.Count; index++)
        {
            ProductDto dto = importResult.Items[index];

            try
            {
                products.Add(Product.FromDto(dto));
            }
            catch (ArgumentException exception)
            {
                errors.Add($"запис {index + 1} ({dto.Id}): {exception.Message}");
            }
        }

        return new ImportResult<Product>(products, errors);
    }
}
