using Core.Domain;
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

Console.WriteLine();
Console.WriteLine("=== Лабораторна 4: успішний сценарій ===");

Customer customerEntity = Customer.Create(
    "C-001",
    "Олена Коваль",
    "olena@example.com",
    "+380501112233",
    "Київ");
Product laptop = Product.Create(
    "P-001",
    "Ноутбук",
    "Комп'ютери",
    32999.99m,
    "15.6 дюймів, 16 ГБ RAM");
Product mouse = Product.Create(
    "P-002",
    "Бездротова миша",
    "Аксесуари",
    849.50m);

Order order = Order.Create("O-001", customerEntity.Id);
order.AddLine(laptop, 1);
order.AddLine(mouse, 2);

Console.WriteLine(order);
foreach (OrderLine line in order.Lines)
{
    Console.WriteLine($"  {line}");
}

OrderConfirmationService.Confirm(order, customerEntity);
Console.WriteLine($"Після підтвердження: {order}");

OrderDto dto = order.ToDto();
Order restoredOrder = Order.FromDto(dto);
Console.WriteLine($"Відновлено з DTO: {restoredOrder}");
Console.WriteLine($"Товар із DTO: {Product.FromDto(laptop.ToDto())}");
Console.WriteLine($"Клієнт із DTO: {Customer.FromDto(customerEntity.ToDto())}");

Console.WriteLine();
Console.WriteLine("=== Лабораторна 4: порушення інваріантів ===");

TryDo(
    "додавання рядка до підтвердженого замовлення",
    () => order.AddLine("P-003", "Клавіатура", 2799.00m, 1));
TryDo(
    "підтвердження порожнього замовлення",
    () => Order.Create("O-002", "C-002").Confirm());
TryDo(
    "нульова кількість товару",
    () => Order.Create("O-003", "C-003").AddLine("P-004", "Монітор", 10999.90m, 0));
TryDo(
    "відновлення DTO з порожнім ID клієнта",
    () => Order.FromDto(new OrderDto("O-004", " ", "Draft", [])));
TryDo(
    "від'ємна ціна товару",
    () => Product.Create("P-005", "Гарнітура", "Аксесуари", -1));
TryDo(
    "клієнт без контактів",
    () => Customer.Create("C-005", "Іван Петренко"));
TryDo(
    "підтвердження замовлення іншим клієнтом",
    () => OrderConfirmationService.Confirm(
        Order.Create("O-005", customerEntity.Id),
        Customer.Create("C-999", "Інший клієнт", "other@example.com")));
TryDo("повторне підтвердження", order.Confirm);

Order cancelledOrder = Order.Create("O-006", customerEntity.Id);
cancelledOrder.Cancel();
Console.WriteLine($"Скасоване замовлення: {cancelledOrder}");

ImportResult<ProductDto> importedDtos = new(
    [
        laptop.ToDto(),
        new ProductDto("P-INVALID", "Некоректний товар", "Аксесуари", -10)
    ],
    ["помилка попереднього етапу імпорту"]);
ImportResult<Product> domainImport = ProductDomainMapper.Map(importedDtos);
Console.WriteLine(
    $"Мапінг імпорту: сутностей {domainImport.Items.Count}, " +
    $"помилок {domainImport.Errors.Count}");
foreach (string error in domainImport.Errors)
{
    Console.WriteLine($" ! {error}");
}

Console.WriteLine($"Стан першого замовлення після відмов: {order}");

return 0;

static OrderImportResult FromProducts(ImportResult<ProductDto> result) =>
    new(result.Items, [], result.Errors);

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток не спрацював");
    }
    catch (Exception exception)
    {
        Console.WriteLine(
            $" {title}: {exception.GetType().Name} — {exception.Message}");
    }
}
