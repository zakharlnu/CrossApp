namespace Api.Contracts;

public sealed record OrderDto(
    string Id,
    string CustomerId,
    DateTimeOffset CreatedAt,
    string Status,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total);

public sealed record OrderLineDto(
    string ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    decimal Total);
