using Cli.Notifications;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Import;
using Core.Reports;
using Core.Services;
using Core.Storage;

string? commandOrPath =
    args.FirstOrDefault(argument => !argument.StartsWith("--"));
bool hasReportCommand =
    commandOrPath?.StartsWith("report-", StringComparison.Ordinal) == true;
string? importPath = hasReportCommand ? null : commandOrPath;
string reportCommand = hasReportCommand ? commandOrPath! : "report-top";
string path = importPath is not null
    ? importPath
    : Path.Combine("data", "sample.csv");
bool isCsv = Path.GetExtension(path).Equals(
    ".csv",
    StringComparison.OrdinalIgnoreCase);

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

OrderImportResult? result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => OrderCsvImporter.Load(
        path,
        error => Console.WriteLine($"[ПРОПУЩЕНО] {error}")),
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

    if (!isCsv)
    {
        foreach (string error in result.Errors)
        {
            Console.WriteLine($" ! {error}");
        }
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
Console.WriteLine("==================================================");
Console.WriteLine("==================================================");
Console.WriteLine("==================================================");
Console.WriteLine("==================================================");
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
Console.WriteLine("==================================================");
Console.WriteLine("==================================================");
Console.WriteLine("==================================================");
Console.WriteLine("==================================================");
Console.WriteLine("==================================================");
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

Console.WriteLine();
Console.WriteLine("==================================================");
Console.WriteLine("=== Лабораторна 5: сервіс і сховища ===");

bool useFileStore = args.Contains("--file");
string ordersPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");
IOrderStore orderStore = useFileStore
    ? new FileOrderStore(ordersPath)
    : new InMemoryOrderStore(SampleData.Orders());

string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
File.WriteAllText(logPath, $"[ЛОГ] {DateTime.Now:yyyy-MM-dd HH:mm:ss} — початок логування{Environment.NewLine}");

string sinkLogPath = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");
INotificationSink notificationSink = new FileSink(sinkLogPath);
var orderService = new OrderService(orderStore, notificationSink);

void OnOrderChanged(object? sender, OrderChangedEventArgs eventArgs)
{
    string line =
        $"[ПОДІЯ] {eventArgs.At:HH:mm:ss} {eventArgs.Kind,-10} " +
        $"{eventArgs.OrderId} позицій: {eventArgs.LinesCount}, " +
        $"сума: {eventArgs.Total:F2}";

    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

orderService.Changed += OnOrderChanged;

int changes = 0;
orderService.Changed += (_, _) => changes++;

Console.WriteLine($"Сховище: {orderStore.GetType().Name}");
Console.WriteLine($"Notification sink: {notificationSink.GetType().Name}");
Console.WriteLine($"Файл notification sink: {sinkLogPath}");
if (useFileStore)
{
    Console.WriteLine($"Файл: {ordersPath}");
}

Console.WriteLine();
Console.WriteLine("==================================================");
Console.WriteLine("=== Лабораторна 6: події та делегати ===");

Order createdOrder = orderService.CreateOrder(customerEntity.Id);
orderService.AddLine(createdOrder.Id, laptop, 1);
orderService.AddLine(createdOrder.Id, mouse, 2);
orderService.Confirm(createdOrder.Id, customerEntity);

Console.WriteLine($"Створено та змінено: {createdOrder}");
Console.WriteLine($"Подій отримано: {changes}");
Console.WriteLine($"Усього в сховищі: {orderService.All().Count}");
foreach (Order storedOrder in orderService.All().Take(5))
{
    Console.WriteLine($" {storedOrder}");
}

Order? foundOrder = orderService.Find(createdOrder.Id);
Console.WriteLine($"Знайдено за id: {foundOrder}");

Console.WriteLine();
Console.WriteLine("--- Перевірка відписки через -= ---");
Console.WriteLine($"До операції: статус {createdOrder.Status}, подій {changes}");
orderService.Changed -= OnOrderChanged;
Console.WriteLine("Початок Cancel() — рядка [ПОДІЯ] Cancelled нижче не буде.");
orderService.Cancel(createdOrder.Id);
Console.WriteLine("Кінець Cancel().");
Console.WriteLine($"Після операції: статус {createdOrder.Status}, подій {changes}");
Console.WriteLine(
    "Замовлення скасовано: [ПОДІЯ] не з'явилась, app.log не змінився.");

int changesBeforeFailure = changes;
TryDo(
    "пошук відсутнього замовлення для зміни",
    () => orderService.AddLine("missing-id", laptop, 1));
Console.WriteLine(
    $"Подій після помилки: {changes} (до помилки: {changesBeforeFailure})");

IReadOnlyList<Order> expensiveOrders =
    orderService.FindAll(storedOrder => storedOrder.Total >= 10_000m);
Console.WriteLine($"Замовлень на суму від 10000: {expensiveOrders.Count}");
Console.WriteLine($"Лог подій: {logPath}");

Console.WriteLine();
Console.WriteLine("==================================================");
Console.WriteLine("=== Лабораторна 7: звіти через LINQ ===");

IReadOnlyList<Order> reportOrders = orderService.All();

switch (reportCommand)
{
    case "report-top":
        Console.WriteLine("Топ-5 клієнтів за сумою замовлень:");
        PrintCustomers(ReportService.TopCustomersByTotal(reportOrders, 5));
        break;
    case "report-top-orders":
        Console.WriteLine("Топ-5 замовлень за сумою:");
        PrintOrders(ReportService.TopOrdersByTotal(reportOrders, 5));
        break;
    case "report-expensive":
        Console.WriteLine("Замовлення дорожче 10000:");
        PrintOrders(ReportService.ExpensiveOrders(reportOrders, 10_000m));
        break;
    case "report-groups":
        Console.WriteLine("Замовлення за статусами:");
        PrintStatuses(ReportService.ByStatus(reportOrders));
        break;
    case "report-sum":
        PrintTotals(ReportService.Totals(reportOrders));
        break;
    case "report-deferred":
        DemonstrateDeferredExecution(orderService);
        break;
    case "report-customers":
        Console.WriteLine("Клієнти та підсумки їхніх замовлень:");
        PrintCustomerOrders(ReportService.ByCustomer(
            SampleData.Customers(),
            reportOrders));
        break;
    case "report-benchmark":
        RunReportBenchmark();
        break;
    default:
        Console.WriteLine(
            "report-top | report-top-orders | report-expensive | " +
            "report-groups | report-sum | report-deferred | " +
            "report-customers | report-benchmark");
        break;
}

return 0;

static void PrintCustomers(IReadOnlyList<CustomerSummary> summaries)
{
    Console.WriteLine($" {"Клієнт",-10} {"Замовлень",10} {"Загальна сума",15}");

    foreach (CustomerSummary summary in summaries)
    {
        Console.WriteLine(
            $" {summary.CustomerId,-10} {summary.OrdersCount,10} " +
            $"{summary.TotalSpent,15:F2}");
    }
}

static void PrintOrders(IReadOnlyList<OrderReportLine> lines)
{
    Console.WriteLine(
        $" {"ID",-10} {"Клієнт",-10} {"Статус",-10} " +
        $"{"Позицій",8} {"Сума",12}");

    foreach (OrderReportLine line in lines)
    {
        Console.WriteLine(
            $" {line.OrderId,-10} {line.CustomerId,-10} {line.Status,-10} " +
            $"{line.LinesCount,8} {line.Total,12:F2}");
    }
}

static void PrintStatuses(IReadOnlyList<StatusSummary> summaries)
{
    Console.WriteLine($" {"Статус",-10} {"Замовлень",10} {"Загальна сума",15}");

    foreach (StatusSummary summary in summaries)
    {
        Console.WriteLine(
            $" {summary.Status,-10} {summary.OrdersCount,10} " +
            $"{summary.TotalAmount,15:F2}");
    }
}

static void PrintCustomerOrders(IReadOnlyList<CustomerOrderSummary> summaries)
{
    Console.WriteLine(
        $" {"ID",-8} {"Клієнт",-22} {"Замовлень",10} {"Загальна сума",15}");

    foreach (CustomerOrderSummary summary in summaries)
    {
        Console.WriteLine(
            $" {summary.CustomerId,-8} {summary.CustomerName,-22} " +
            $"{summary.OrdersCount,10} {summary.TotalSpent,15:F2}");
    }
}

static void PrintTotals(
    (int Orders, decimal Total, decimal Average, decimal Max) totals)
{
    Console.WriteLine(
        $"Замовлень: {totals.Orders}, загальна сума: {totals.Total:F2}, " +
        $"середній чек: {totals.Average:F2}, максимум: {totals.Max:F2}");
}

static void DemonstrateDeferredExecution(OrderService service)
{
    List<Order> source = service.All().ToList();
    IEnumerable<OrderReportLine> query =
        ReportService.ExpensiveOrdersQuery(source, 10_000m);

    Console.WriteLine($"Дорогих замовлень до зміни: {query.Count()}");

    Order addedOrder = Order.Create("TMP-001", "C-TMP");
    addedOrder.AddLine("P-TMP", "Демонстраційний товар", 50_000m, 1);
    source.Add(addedOrder);

    Console.WriteLine($"Дорогих замовлень після зміни: {query.Count()}");

    List<OrderReportLine> snapshot = query.ToList();
    source.Clear();

    Console.WriteLine($"У знімку залишилося: {snapshot.Count}");
}

static void RunReportBenchmark()
{
    const int count = 100_000;
    var orders = new List<Order>(count);

    for (int index = 1; index <= count; index++)
    {
        Order order = Order.Create(
            $"B-{index:D6}",
            $"C-{index % 1_000:D4}");
        order.AddLine(
            $"P-{index % 100:D3}",
            "Тестовий товар",
            index % 20_000 + 1,
            1);
        orders.Add(order);
    }

    ReportBenchmarkResult result =
        ReportService.BenchmarkMaterialization(orders, 10_000m);

    Console.WriteLine($"Вхідних замовлень: {result.SourceCount}");
    Console.WriteLine($"У результаті: {result.ResultCount}");
    Console.WriteLine(
        $"Без проміжного ToList: " +
        $"{result.WithoutIntermediateToList.TotalMilliseconds:F2} мс");
    Console.WriteLine(
        $"З проміжним ToList: " +
        $"{result.WithIntermediateToList.TotalMilliseconds:F2} мс");
}

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
