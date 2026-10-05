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

# Імпорт з іншого CSV-файлу
dotnet run --project src/Cli/Cli.csproj -- path/to/orders.csv

# Імпорт товарів із JSON-файлу
dotnet run --project src/Cli/Cli.csproj -- data/sample.json

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

## Середовище
* .NET SDK 8.0
* Arch Linux x64

## Структура проекту
```text
CrossApp
├── data
│   ├── sample.csv
│   └── sample.json
├── CrossApp.slnx
├── README.md
└── src
    ├── Core
    │   ├── Dto
    │   │   ├── CustomerDto.cs
    │   │   ├── ImportResult.cs
    │   │   ├── OrderImportResult.cs
    │   │   └── ProductDto.cs
    │   ├── Import
    │   │   ├── OrderCsvImporter.cs
    │   │   └── ProductJsonImporter.cs
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   └── EnvironmentReport.cs
    └── Cli
        ├── Cli.csproj
        └── Program.cs
```
`Core` - class library, який містить основну логіку та сутності предметної області.

`Cli` - console application, який використовує Core для демонстрації роботи.

## Формат даних

Товари та клієнти імпортуються з CSV-файлу в кодуванні UTF-8. Роздільник — крапка
з комою, перший рядок може містити заголовок `type;id;name;value;details;note`.
Для товару ці колонки означають ціну, категорію та опис. Для клієнта — електронну
адресу, телефон та адресу проживання. Префікс `P` позначає товар, а `C` — клієнта.
Ціна записується з крапкою як десятковим роздільником. `data/sample.csv` містить
10 коректних записів і 3 навмисно пошкоджені рядки для демонстрації помилок.

Для JSON підтримується масив товарів із полями `id`, `name`, `category`, `price` і
необов'язковим `description`. CLI автоматично обирає імпортер за розширенням `.csv`
або `.json`. Некоректні JSON-об'єкти повертаються як помилки з номерами елементів,
а коректні товари продовжують імпортуватися.

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
