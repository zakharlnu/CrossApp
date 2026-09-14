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
├── CrossApp.slnx
├── README.md
└── src
    ├── Core
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   └── EnvironmentReport.cs
    └── Cli
        ├── Cli.csproj
        └── Program.cs
```
`Core` - class library, який містить основну логіку та сутності предметної області.

`Cli` - console application, який використовує Core для демонстрації роботи.

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
