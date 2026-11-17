using Core.Domain;
using Core.Dto;

namespace Core.Import;

public sealed record OrderLineImport(string OrderId, OrderLine Line)
{
    public static OrderLineImport FromDto(OrderLineImportDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.OrderId))
        {
            throw new ArgumentException(
                "Ідентифікатор замовлення не може бути порожнім",
                nameof(dto));
        }

        var lineDto = new OrderLineDto(
            dto.ProductId,
            dto.ProductName,
            dto.Price,
            dto.Quantity);

        return new OrderLineImport(
            dto.OrderId.Trim(),
            OrderLine.FromDto(lineDto));
    }
}
