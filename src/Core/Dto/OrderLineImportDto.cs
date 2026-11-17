namespace Core.Dto;

public sealed record OrderLineImportDto(
    string OrderId,
    string ProductId,
    string ProductName,
    decimal Price,
    int Quantity);
