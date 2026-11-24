# CrossApp

Наскрізний проєкт з крос-платформного програмування.

**Предметна область:** Замовлення.

**Сутності:** `Customer`, `Product`, `Order`, `OrderLine`.

**Призначення:** оформлення замовлень та розрахунок їхньої загальної вартості.

## Usage 

```bash
# Збірка всіх проектів солюшина
dotnet build

# Збірка окремих проектів
dotnet build src/Cli/Cli.csproj
dotnet build src/Core/Core.csproj

# Запуск CLI проекту
dotnet run --project src/Cli/Cli.csproj

# Запуск із файловим сховищем замовлень
dotnet run --project src/Cli/Cli.csproj -- --file

# Імпорт з іншого CSV-файлу
dotnet run --project src/Cli/Cli.csproj -- path/to/orders.csv

# Імпорт товарів із JSON-файлу
dotnet run --project src/Cli/Cli.csproj -- data/sample.json

# Звіти лабораторної 7
dotnet run --project src/Cli/Cli.csproj -- report-top
dotnet run --project src/Cli/Cli.csproj -- report-top-orders
dotnet run --project src/Cli/Cli.csproj -- report-expensive
dotnet run --project src/Cli/Cli.csproj -- report-groups
dotnet run --project src/Cli/Cli.csproj -- report-sum
dotnet run --project src/Cli/Cli.csproj -- report-deferred
dotnet run --project src/Cli/Cli.csproj -- report-customers
dotnet run --project src/Cli/Cli.csproj -- report-benchmark

# Звіт із файловим сховищем
dotnet run --project src/Cli/Cli.csproj -- report-groups --file

# Асинхронний імпорт рядків замовлень (лабораторна 8)
dotnet run --project src/Cli/Cli.csproj -- import-async

# Потоковий імпорт через IAsyncEnumerable
dotnet run --project src/Cli/Cli.csproj -- import-stream

# Явна демонстрація скасування
dotnet run --project src/Cli/Cli.csproj -- import-async --cancel

# Демонстрація CancelAfter під час потокового імпорту
dotnet run --project src/Cli/Cli.csproj -- import-stream --timeout-demo

# Демонстрація доменної помилки з номером запису
dotnet run --project src/Cli/Cli.csproj -- import-async data/order-lines-invalid.json

# Unit-тести Core
dotnet test CrossApp.slnx

# Unit-тести з покриттям
dotnet test tests/Core.Tests/Core.Tests.csproj --collect:"XPlat Code Coverage"

# Експорт усіх замовлень у налаштований JSON-файл
dotnet run --project src/Cli/Cli.csproj -- export

# Імпорт усіх замовлень із налаштованого JSON-файлу
dotnet run --project src/Cli/Cli.csproj -- import

# HTTP API лабораторної 10 (порт зафіксовано в launchSettings.json)
dotnet run --project src/Api/Api.csproj

# OpenAPI та Scalar доступні лише в Development
# http://localhost:5080/openapi/v1.json
# http://localhost:5080/scalar/v1

# CROSSAPP_DATA_PATH перекриває Storage:DataPath з appsettings.json
CROSSAPP_DATA_PATH=/tmp/crossapp-data \
  dotnet run --project src/Cli/Cli.csproj -- export

# Імпорт із того самого перевизначеного каталогу
CROSSAPP_DATA_PATH=/tmp/crossapp-data \
  dotnet run --project src/Cli/Cli.csproj -- import

CROSSAPP_Storage__DataPath=/tmp/crossapp-data \
  dotnet run --project src/Cli/Cli.csproj -- export

# Self-contained publish
dotnet publish src/Cli/Cli.csproj \
    -c Release \
    -f net10.0 \
    -r linux-x64 \
    --self-contained true

# Framework-dependent publish
dotnet publish src/Cli/Cli.csproj \
    -c Release \
    -f net10.0 \
    -r linux-x64 \
    --self-contained false

# Запуск publish
./src/Cli/bin/Release/net10.0/linux-x64/publish/Cli
```

## HTTP API

### SQLite і міграції (лабораторна 11)

API зберігає замовлення і їхні рядки у SQLite (`Orders` 1:N `OrderLines`).
Рядок підключення береться з `ConnectionStrings:AppDb` у
`src/Api/appsettings.json`: `Data Source=data/app.db`. Відносний шлях
обчислюється від `ContentRootPath` API (`src/Api` при запуску з проєкту),
тому типовий файл — `src/Api/data/app.db`; каталог створюється автоматично.
Можна перевизначити рядок через `ConnectionStrings__AppDb`.
У Development міграції застосовуються при запуску API; в інших середовищах
застосуйте їх окремою командою.

```bash
dotnet tool install --global dotnet-ef --version "10.*"
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Api --output-dir Migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Api
dotnet ef migrations list --project src/Infrastructure --startup-project src/Api
sqlite3 src/Api/data/app.db '.tables'
```

`InitialCreate` уже є в проєкті; команду `migrations add` потрібно виконувати
тільки при створенні нової міграції з іншою назвою. Щоб скинути локальну базу,
зупиніть API, видаліть `src/Api/data/app.db` разом із можливими
`src/Api/data/app.db-wal` і `src/Api/data/app.db-shm`, тоді запустіть
`database update`. Це видалить локальні записи.
Файл бази не додається в Git, а міграції зберігають відтворювану схему.

API використовує окремі DTO та надає п'ять маршрутів для замовлень:

| Метод | Маршрут | Успішна відповідь | Помилки |
|---|---|---|---|
| `GET` | `/api/orders` | `200 OK` | — |
| `GET` | `/api/orders/{id}` | `200 OK` | `404 Not Found` |
| `POST` | `/api/orders` | `201 Created` | `400 Bad Request` |
| `PUT` | `/api/orders/{id}` | `200 OK` | `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/orders/{id}` | `204 No Content` | `404 Not Found` |

Приклад створення замовлення:

```bash
curl -i -X POST http://localhost:5080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "C-101",
    "lines": [
      {
        "productId": "P-101",
        "productName": "Ноутбук",
        "price": 32999.99,
        "quantity": 1
      }
    ]
  }'
```

Скопіюйте `id` із відповіді та виконайте повний CRUD-цикл:

```bash
curl -i http://localhost:5080/api/orders/ORDER_ID

curl -i -X PUT http://localhost:5080/api/orders/ORDER_ID \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "C-101",
    "lines": [
      {
        "productId": "P-102",
        "productName": "Монітор",
        "price": 10999.90,
        "quantity": 2
      }
    ]
  }'

curl -i -X DELETE http://localhost:5080/api/orders/ORDER_ID
curl -i http://localhost:5080/api/orders/ORDER_ID
```

Приклад помилки валідації з відповіддю `400 ValidationProblemDetails`:

```bash
curl -i -X POST http://localhost:5080/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"","lines":[]}'
```

Вихідний `OrderDto` відокремлює стабільний HTTP-контракт від доменної сутності.
Клієнт не отримує внутрішні методи та правила агрегату, а зміни реалізації домену не
повинні автоматично змінювати JSON API.

## Середовище
* .NET SDK 10.0
* Arch Linux x64

## Структура проекту
```text
CrossApp
├── data
│   ├── order-lines-invalid.json
│   ├── order-lines.json
│   ├── sample.csv
│   └── sample.json
├── CrossApp.slnx
├── README.md
├── src
│   ├── Core
│   │   ├── Abstractions
│   │   │   ├── IOrderLineImporter.cs
│   │   │   ├── IStreamingOrderLineImporter.cs
│   │   │   ├── INotificationSink.cs
│   │   │   └── IOrderStore.cs
│   │   ├── Configuration
│   │   │   └── AppOptions.cs
│   │   ├── Domain
│   │   │   ├── Customer.cs
│   │   │   ├── Order.cs
│   │   │   ├── OrderConfirmationService.cs
│   │   │   ├── OrderLine.cs
│   │   │   ├── OrderStatus.cs
│   │   │   └── Product.cs
│   │   ├── Dto
│   │   │   ├── CustomerDto.cs
│   │   │   ├── ImportResult.cs
│   │   │   ├── OrderImportResult.cs
│   │   │   ├── OrderDto.cs
│   │   │   ├── OrderLineDto.cs
│   │   │   ├── OrderLineImportDto.cs
│   │   │   └── ProductDto.cs
│   │   ├── Import
│   │   │   ├── JsonOrderLineImporter.cs
│   │   │   ├── OrderCsvImporter.cs
│   │   │   ├── OrderLineImport.cs
│   │   │   ├── ProductDomainMapper.cs
│   │   │   └── ProductJsonImporter.cs
│   │   ├── Events
│   │   │   ├── OrderChangedEventArgs.cs
│   │   │   └── OrderChangeKind.cs
│   │   ├── Reports
│   │   │   ├── CustomerOrderSummary.cs
│   │   │   ├── CustomerSummary.cs
│   │   │   ├── EnumerableExtensions.cs
│   │   │   ├── OrderReportLine.cs
│   │   │   ├── ReportBenchmarkResult.cs
│   │   │   ├── ReportService.cs
│   │   │   └── StatusSummary.cs
│   │   ├── Services
│   │   │   └── OrderService.cs
│   │   ├── Storage
│   │   │   ├── FileOrderStore.cs
│   │   │   └── InMemoryOrderStore.cs
│   │   ├── Core.csproj
│   │   ├── DomainException.cs
│   │   ├── EnvironmentInfo.cs
│   │   ├── EnvironmentReport.cs
│   │   └── SampleData.cs
│   ├── Api
│   │   ├── Contracts
│   │   │   ├── OrderDto.cs
│   │   │   ├── OrderMappings.cs
│   │   │   └── SaveOrderRequest.cs
│   │   ├── Endpoints
│   │   │   └── OrderEndpoints.cs
│   │   ├── Errors
│   │   │   └── BadHttpRequestExceptionHandler.cs
│   │   ├── Properties
│   │   │   └── launchSettings.json
│   │   ├── Api.csproj
│   │   ├── Api.http
│   │   ├── appsettings.json
│   │   └── Program.cs
│   └── Cli
│       ├── Import
│       │   └── DelayedStreamingOrderLineImporter.cs
│       ├── Notifications
│       │   ├── ConsoleSink.cs
│       │   └── FileSink.cs
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       ├── appsettings.json
│       ├── Cli.csproj
│       └── Program.cs
└── tests
    └── Core.Tests
        ├── AdditionalAsyncTests.cs
        ├── ImportTests.cs
        ├── OrderServiceTests.cs
        ├── ReportServiceTests.cs
        └── Core.Tests.csproj
```
`Core` - class library, який містить основну логіку та сутності предметної області.

`Cli` - console application, який використовує Core для демонстрації роботи.

`Api` - Minimal API, який надає HTTP CRUD над замовленнями з Core. Сховище
`InMemoryOrderStore` реєструється як Singleton у DI; тому доступ до його словника
синхронізований. `Scoped` створював би один екземпляр на HTTP-запит, а `Transient` —
новий під час кожного отримання залежності, що для сховища в пам'яті втрачало б стан.

## Формат даних

Товари та клієнти імпортуються з CSV-файлу в кодуванні UTF-8. Роздільник — крапка
з комою, перший рядок може містити заголовок `type;id;name;value;details;note`.
Для товару ці колонки означають ціну, категорію та опис. Для клієнта — електронну
адресу, телефон та адресу проживання. Префікс `P` позначає товар, а `C` — клієнта.
Ціна записується з крапкою як десятковим роздільником. `data/sample.csv` містить
коректні та навмисно пошкоджені рядки для демонстрації помилок.

Для JSON підтримується масив товарів із полями `id`, `name`, `category`, `price` і
необов'язковим `description`. CLI автоматично обирає імпортер за розширенням `.csv`
або `.json`. Некоректні JSON-об'єкти повертаються як помилки з номерами елементів,
а коректні товари продовжують імпортуватися.

`OrderCsvImporter.Load` приймає необов'язковий `Action<string>`. Імпортер викликає
його для кожного пропущеного рядка, а CLI визначає конкретну реакцію лямбдою.
Список помилок також залишається в `OrderImportResult` для підсумкової статистики.

## Доменна модель замовлення

`Order` є агрегатом, який володіє колекцією `OrderLine` і посилається на клієнта за
ідентифікатором. Колекція доступна назовні лише для читання, а змінювати замовлення
можна через методи `AddLine` і `Confirm`. `Product` контролює ціну та опис товару,
а `Customer` — контактні дані й адресу. DTO використовуються для перенесення даних,
а доменні класи захищають бізнес-правила.

Інваріанти доменної моделі:

- ідентифікатори замовлення, клієнта й товару не можуть бути порожніми;
- назва товару не може бути порожньою;
- товар повинен мати категорію;
- ціна товару не може бути від'ємною;
- клієнт повинен мати хоча б один контакт: email або телефон;
- кількість товару в рядку має бути більшою за нуль;
- до підтвердженого замовлення не можна додавати рядки;
- порожнє або вже підтверджене замовлення не можна підтвердити;
- дозволені переходи стану: `Draft → Confirmed`, `Draft → Cancelled` і
  `Confirmed → Cancelled`;
- замовлення може підтвердити лише клієнт, ідентифікатор якого записаний у замовленні;
- `FromDto` відновлює сутності через ті самі фабрики й методи, тому не обходить перевірки.

Правило відповідності замовлення та клієнта перевіряє `OrderConfirmationService`,
оскільки воно охоплює дві сутності. Такі правила розміщують у сервісі, щоб жодна
сутність не відповідала за завантаження та життєвий цикл іншої.

`ProductDomainMapper` перетворює `ImportResult<ProductDto>` на `ImportResult<Product>`:
зберігає помилки читання файлу та додає помилки доменних інваріантів.

## Сервісний шар і сховища

`IOrderStore` визначає операції отримання списку, пошуку, додавання, оновлення та
видалення замовлень. Його реалізують `InMemoryOrderStore` і `FileOrderStore`.
`OrderService` залежить від `IOrderStore` та `INotificationSink` і надає операції
`CreateOrder`, `AddLine`, `Confirm`, `Cancel`, `All`, `Find` і `FindAll`.

Залежності мають такий напрямок:

```text
Cli → OrderService → IOrderStore ← InMemoryOrderStore
                                  ← FileOrderStore
```

Без аргументу `--file` використовуються 15 демонстраційних замовлень у пам'яті, які
зникають після завершення процесу. З аргументом `--file` агрегати зберігаються як
`OrderDto` разом із рядками у `data/orders.json` біля виконуваного файла. Повторний
запуск завантажує раніше збережені замовлення.

## Доменні події та делегати

`OrderService` піднімає подію `Changed` після успішного створення замовлення,
додавання позиції, підтвердження або скасування. `OrderChangedEventArgs` передає
ідентифікатор замовлення, вид зміни, кількість позицій, загальну суму та час події.
Якщо доменний інваріант або операція сховища завершується винятком, подія не
піднімається.

CLI підписує на подію локальний метод для виведення в консоль і запису в
`logs/app.log`, а також лямбду, яка через замикання рахує отримані повідомлення.
Після відписки локального методу операції продовжують змінювати дані, але консольний
і файловий обробник більше не запускається. Метод `FindAll` приймає
`Func<Order, bool>`, тому умову фільтрації задає викликач.

### Notification sink і подія

`INotificationSink` є явною залежністю `OrderService`, яку CLI передає через
конструктор. `ConsoleSink` виводить повідомлення в консоль, а `FileSink` записує
їх у заданий через конструктор файл. Зараз у composition root використовується
`FileSink` із файлом `logs/sink.log`; його можна замінити на `ConsoleSink` без
змін в `OrderService`.

Подія дає змогу довільній кількості зовнішніх компонентів підписуватися і
відписуватися під час роботи. Sink є обов'язковим одержувачем, визначеним під час
створення сервісу. Тому подія зручна для необов'язкових спостерігачів, а інтерфейс
sink — коли повідомлення є явною залежністю сценарію.

### Чому асинхронний обробник події небезпечний

`EventHandler<TEventArgs>` повертає `void`, тому асинхронна лямбда, підписана на
таку подію, стає `async void`. Видавець не отримує `Task` і не може дочекатися
завершення обробника. Виняток, який виник після `await`, не передається виклику
сервісу звичайним шляхом і може завершити процес. Кілька таких обробників також
можуть завершитися не в порядку підняття подій.

Якщо асинхронне повідомлення потрібно обов'язково завершити, доцільніше використати
метод `NotifyAsync`, який повертає `Task`, або делегат
`Func<OrderChangedEventArgs, Task>`, і явно виконати `await`. Якщо залишається
`async void`, винятки потрібно перехоплювати всередині самого обробника.

## Звіти через LINQ

`ReportService` приймає `IEnumerable<Order>` і не залежить від сховища або CLI.
Готові звіти матеріалізуються в `IReadOnlyList` незмінних record-DTO, тому зовнішній
код не отримує доменні сутності та не залежить від їхньої внутрішньої структури.

Реалізовані запити:

- `TopCustomersByTotal` відповідає, які п'ять клієнтів мають найбільшу загальну
  суму замовлень; використовує `GroupBy`, `Sum`, `OrderByDescending`, `ThenBy`,
  `Take` і `Select`;
- `ExpensiveOrders` знаходить замовлення дорожче заданої суми; використовує
  `Where`, два ключі сортування і проєкцію в `OrderReportLine`;
- `ByStatus` показує кількість і загальну суму замовлень для кожного статусу;
  використовує `GroupBy`, `Count`, `Sum`, `Select` і сортування;
- `Totals` обчислює кількість замовлень, загальну суму, середній чек і максимальне
  замовлення. Для порожнього джерела він повертає нулі, тому `Average` і `Max` не
  кидають `InvalidOperationException`.

`ExpensiveOrdersQuery` повертає відкладений LINQ-запит: під час його створення
`Where`, `OrderBy` і `Select` ще не перебирають джерело. Кожен виклик `Count()`
запускає запит заново, тому після додавання дорогого замовлення до локального списку
другий результат збільшується на один. Виклик `ToList()` матеріалізує знімок, тому
після очищення початкового списку кількість елементів у знімку не змінюється.

`SampleData` містить 15 замовлень із різними сумами та трьома статусами: `Draft`,
`Confirmed` і `Cancelled`. Цього достатньо, щоб топ, фільтр і групування давали
непорожні та наочні результати.

### Додаткові завдання

`ByCustomer` виконує `GroupJoin` колекцій клієнтів і замовлень за ідентифікатором
клієнта. На відміну від звичайного `Join`, він залишає у звіті навіть клієнта без
замовлень: для нього кількість і сума дорівнюють нулю. Результат проєктується в
`CustomerOrderSummary`, тому назовні не передаються доменні сутності.

Метод-розширення `TopBy` містить спільну логіку сортування за показником,
додатковим рядковим ключем і вибору перших N елементів. Його використовують звіти
`TopCustomersByTotal` і `TopOrdersByTotal`; додатковий ключ робить результат
детермінованим, якщо суми однакові.

`ByStatus` записаний у синтаксисі запитів `from ... group ... into ...`. Для
групування та обчислення кількох агрегатів він читається послідовно: спочатку група,
потім сума, сортування і проєкція. Методний синтаксис зручніший для коротких
ланцюжків на кшталт `Where` → `OrderBy` → `Select`, тому решта звітів залишена в
ньому. Обидва записи компілюються у виклики тих самих LINQ-методів.

Команда `report-benchmark` створює 100 000 замовлень і через `Stopwatch` порівнює
однаковий звіт без проміжного `ToList` та з матеріалізацією одразу після `Where`.
Обидва варіанти повертають однакову кількість рядків, але проміжний список потребує
додаткової пам'яті й проходу по відібраних елементах. Конкретний час залежить від
машини, стану JIT і навантаження, тому це демонстраційне вимірювання, а не точний
мікробенчмарк.

## Асинхронний імпорт і unit-тести

`JsonOrderLineImporter.LoadAsync` асинхронно читає масив рядків замовлень через
`FileStream` з `useAsync: true` та `JsonSerializer.DeserializeAsync`. Метод не
використовує `Task.Run`, `.Result` або `.Wait()`. Токен скасування передається в
`DeserializeAsync` і додатково перевіряється перед відкриттям файлу та в циклі
перетворення записів.

`OrderService.ImportAsync` очікує імпортер через `IOrderLineImporter`, тому його
можна тестувати без диска. Сервіс передає токен імпортеру, перевіряє його в циклі,
пропускає вже наявний у замовленні товар та після кожного успішного додавання
піднімає `Changed`. Доменні обчислення, зміна стану агрегату та LINQ-звіти
залишилися синхронними, оскільки вони працюють у пам'яті й не очікують I/O.

Фрагмент асинхронного читання:

```csharp
await using var stream = new FileStream(
    path, FileMode.Open, FileAccess.Read, FileShare.Read,
    bufferSize: 4096, useAsync: true);

List<OrderLineImportDto>? dtos =
    await JsonSerializer.DeserializeAsync<List<OrderLineImportDto>>(
        stream, Options, cancellationToken);
```

Фрагмент передачі й перевірки скасування:

```csharp
IReadOnlyList<OrderLineImport> items =
    await importer.LoadAsync(path, cancellationToken);

foreach (OrderLineImport item in items)
{
    cancellationToken.ThrowIfCancellationRequested();
    // додавання рядка до замовлення
}
```

CLI створює `CancellationTokenSource`, а обробник `Console.CancelKeyPress`
перетворює `Ctrl+C` на сигнал кооперативного скасування. `OperationCanceledException`
обробляється окремо й виводить `Імпорт скасовано.`. Прапорець `--cancel` скасовує
токен до читання файлу та дає змогу відтворити цей сценарій на малому файлі.
Для захисту прапорець `--timeout-demo` викликає
`CancelAfter(TimeSpan.FromSeconds(2))`. Потоковий JSON-імпортер читає записи по
одному, а демонстраційний декоратор робить паузу 500 мс перед передаванням кожного
з них у сервіс. Кілька позицій реально додаються та піднімають події, після чого
таймаут скасовує очікування наступного запису. `OperationCanceledException`
обробляється CLI повідомленням `Імпорт скасовано.`. Декоратор розміщений у CLI,
тому штучна затримка не потрапляє в доменний або сервісний код Core.

`DomainException` означає порушення бізнес-правила: наприклад, додавання позиції
до підтвердженого замовлення або відсутність потрібного замовлення. Некоректні
аргументи окремої операції, наприклад від'ємна кількість, залишаються
`ArgumentException` чи `ArgumentOutOfRangeException`. Імпортер обгортає таку
помилку запису в `DomainException` із номером запису та зберігає початковий виняток
в `InnerException`. Технічні `FileNotFoundException` і `JsonException` не
маскуються як доменні та обробляються CLI окремо.

### Тести Core

Проєкт `Core.Tests` посилається лише на `Core`. Кожен тест створює нове сховище в
пам'яті та власні фіксовані дані; каталог `data/` у тестах не читається.

| Тест | Що перевіряє | Очікуваний результат |
|---|---|---|
| `AddLine_WithNegativeQuantity_Throws_AndKeepsState` | інваріант кількості | конкретний виняток, стан не змінено |
| `AddLine_ToConfirmedOrder_ThrowsDomainException_AndKeepsState` | бізнес-правило статусу | `DomainException`, сума й рядки незмінні |
| `AddLine_RaisesChangedExactlyOnce` | подію після успішної зміни | рівно одна подія `LineAdded` |
| `ImportAsync_SkipsExistingProduct_AndRaisesEventForNewLine` | ідемпотентність імпорту | додано один новий рядок і одну подію |
| `ExpensiveOrders_CountsOnlyAboveThreshold` | звіт для трьох порогів | кількість відповідає `InlineData` |
| `TopOrdersByTotal_UsesIdAsDeterministicSecondKey` | детермінований top-N | однакові суми впорядковано за ID |
| `Totals_WithEmptySource_ReturnsZeros` | порожнє джерело | усі агрегати дорівнюють нулю |
| `ImportAsync_WhenAlreadyCancelled_Throws` | передавання скасування через сервіс до імпортера | `OperationCanceledException` |
| `ImportStreamingAsync_AddsLinesFromAsyncSequence` | споживання `IAsyncEnumerable` через `await foreach` | обидва рядки додано |
| `ImportWithTimeoutAsync_CancelsFromTimeoutToken` | linked token і джерело скасування | linked token скасовано, user token активний |
| `ImportAsync_ReportsProgressInPercent` | `IProgress<int>` | значення `0`, `50`, `100` |

Перевірений результат:

```text
Passed! - Failed: 0, Passed: 13, Skipped: 0, Total: 13
```

### Додаткові завдання лабораторної 8

`JsonOrderLineImporter.StreamAsync` повертає `IAsyncEnumerable<OrderLineImport>`.
Він читає JSON-масив через `JsonSerializer.DeserializeAsyncEnumerable`, а атрибут
`[EnumeratorCancellation]` пов'язує токен виклику з асинхронним ітератором.
`OrderService.ImportStreamingAsync` використовує `await foreach`, тому записи
можна перевіряти й додавати по одному без попереднього створення списку всіх
результатів. Сценарій запускається командою `import-stream`.

`ImportWithTimeoutAsync` створює окремий `CancellationTokenSource` для таймауту і
об'єднує його з токеном користувача:

```csharp
using var timeoutSource = new CancellationTokenSource(timeout);
using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
    userToken,
    timeoutSource.Token);
```

Тест `ImportWithTimeoutAsync_CancelsFromTimeoutToken` запускає імпортер, який чекає
до скасування. Після винятку тест перевіряє, що користувацький токен не скасований,
а отриманий імпортером linked token скасований. Отже, сигнал надійшов саме від
таймауту.

Основний `ImportAsync` приймає `IProgress<int>`. Він повідомляє `0` перед циклом і
відсоток після кожного опрацьованого запису. CLI передає `ConsoleProgress`, тому
звичайний запуск показує поступове зростання від `0%` до `100%` для 16
демонстраційних рядків.
`IProgress<int>` не прив'язує Core до консолі: інший клієнт може відобразити цей
самий прогрес у GUI або передати тестову реалізацію.

Команда покриття:

```bash
dotnet test tests/Core.Tests/Core.Tests.csproj --collect:"XPlat Code Coverage"
```

Фактичний результат: покриття рядків Core — `25.72%` (`283/1100`), гілок —
`20.08%` (`46/229`). Не покриті переважно компоненти попередніх лабораторних:
`FileOrderStore`, старі CSV/JSON-імпортери, `EnvironmentInfo`, `EnvironmentReport`
і повний набір `SampleData`. Це прийнятно для цієї роботи, тому що її тести
зосереджені на новій асинхронній межі, скасуванні, доменних інваріантах, подіях і
ключових LINQ-звітах; файлове сховище та старі імпортери потребують окремих
інтеграційних тестів.

## Конфігурація, шляхи та secrets

CLI будує конфігурацію від базового каталогу застосунку `AppContext.BaseDirectory`.
Джерела підключені в такому порядку, і кожне наступне перекриває попереднє:

```text
appsettings.json
→ appsettings.{CROSSAPP_ENVIRONMENT}.json
→ user-secrets
→ змінні середовища з префіксом CROSSAPP_
→ аргументи --Key:NestedKey=value
→ проста CROSSAPP_DATA_PATH для сумісності
```

`AppContext.BaseDirectory` не залежить від каталогу, з якого запустили `dotnet run`.
Тому команди з кореня solution і з `src/Cli` показують той самий файл:

```bash
dotnet run --project src/Cli/Cli.csproj -- export

cd src/Cli
dotnet run -- export
```

За замовчуванням Development використовує файл:

```text
<AppContext.BaseDirectory>/data/development/orders.json
```

Шлях створюється через `Path.Combine`, `Path.GetFullPath` і
`Directory.CreateDirectory`. Роздільники каталогів не записуються вручну, тому код
працює на Linux, Windows і macOS.

### Профілі

| Профіль | Мінімальний лог | Каталог даних |
|---|---|---|
| Development | `Debug` | `data/development` |
| Production | `Warning` | `data/production` |

Профіль вибирає змінна `CROSSAPP_ENVIRONMENT`:

```bash
CROSSAPP_ENVIRONMENT=Production \
  dotnet run --project src/Cli/Cli.csproj -- export
```

```powershell
$env:CROSSAPP_ENVIRONMENT = "Production"
dotnet run --project src/Cli/Cli.csproj -- export
```

### Ключі конфігурації

| Ключ | Значення за замовчуванням | Призначення |
|---|---|---|
| `EnvironmentName` | `Development` | назва активного профілю у виводі |
| `Storage:DataPath` | `data`, далі профіль | каталог JSON-сховища |
| `Storage:FileName` | `orders.json` | ім'я JSON-файлу |
| `Logging:MinimumLevel` | `Information`, далі профіль | мінімальний рівень логування |
| `Orders:Currency` | `UAH` | валюта сум замовлень |
| `Integration:ApiKey` | відсутній | навчальний secret інтеграції |

Для вкладеного ключа двокрапка у змінній середовища замінюється подвійним
підкресленням.

Bash:

```bash
CROSSAPP_Storage__DataPath=/tmp/crossapp-data \
CROSSAPP_Orders__Currency=EUR \
  dotnet run --project src/Cli/Cli.csproj -- export
```

PowerShell:

```powershell
$env:CROSSAPP_Storage__DataPath = "C:\Temp\crossapp-data"
$env:CROSSAPP_Orders__Currency = "EUR"
dotnet run --project src/Cli/Cli.csproj -- export
```

Також підтримується проста змінна, яка має найвищий пріоритет для каталогу:

```bash
CROSSAPP_DATA_PATH=/tmp/crossapp-data \
  dotnet run --project src/Cli/Cli.csproj -- export
```

Command-line override:

```bash
dotnet run --project src/Cli/Cli.csproj -- \
  export --Storage:DataPath=/tmp/crossapp-command
```

### User-secrets

У `Cli.csproj` зберігається лише `UserSecretsId`. Сам secret знаходиться поза
репозиторієм у профілі користувача:

```text
Linux/macOS: ~/.microsoft/usersecrets/<UserSecretsId>/secrets.json
Windows:     %APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json
```

Записати й перевірити навчальний ключ:

```bash
dotnet user-secrets set "Integration:ApiKey" "<your-local-key>" \
  --project src/Cli/Cli.csproj
dotnet user-secrets list --project src/Cli/Cli.csproj
```

У production user-secrets не використовують як централізоване сховище: значення
задають через змінні середовища або спеціальний secret manager:

```bash
CROSSAPP_Integration__ApiKey=<secret-value> \
  dotnet run --project src/Cli/Cli.csproj -- export
```

CLI ніколи не друкує сам ключ — лише `не заданий` або `присутній, довжина N`.
`appsettings.json` не містить паролів або токенів.

### Імпорт, експорт та атомарний запис

Команда `export` бере всі 15 замовлень із `SampleData`, перетворює їх на `OrderDto`
разом із масивами `lines` і записує у файл зі схеми конфігурації. Команда `import`
читає всю колекцію та відновлює доменні сутності через `Order.FromDto`, тому
інваріанти не обходяться.

JSON використовує camelCase, відступи та читабельну кирилицю. Запис виконується
атомарно:

```text
серіалізація → orders.json.tmp → File.Move(..., overwrite: true) → orders.json
```

Якщо запис тимчасового файлу не вдався, старий `orders.json` залишається цілим.
Після успішного запису `.tmp` атомарно замінює основний файл на файлових системах,
що підтримують таку операцію.

## Self-contained publish

| RID | Publish directory size |
|---|---:|
| linux-x64 | 80M |
| win-x64 | 77M |

# Publish
**Self-contained** — під час `publish` .NET Runtime та необхідні runtime-залежності включаються в publish. Застосунок може запускатися без попередньо встановленого .NET Runtime.

**Framework-dependent** — під час `publish` .NET Runtime не включається. Застосунок залежить від сумісного .NET Runtime, встановленого на цільовій машині.

| RID         | Режим               | Розмір publish | Потрібен встановлений runtime |
| ----------- | ------------------- | -------------: | ----------------------------- |
| `linux-x64` | Self-contained      |            80M | Ні                            |
| `linux-x64` | Framework-dependent |           140K | Так (.NET 10)                 |
