namespace Core.Dto;

public sealed record ProductDto(
    string Id,
    string Name,
    string Category,
    decimal Price,
    string? Description = null);
