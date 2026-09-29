namespace Core.Dto;

public sealed record OrderImportResult(
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<CustomerDto> Customers,
    IReadOnlyList<string> Errors);
