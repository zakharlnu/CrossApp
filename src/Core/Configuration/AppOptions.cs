namespace Core.Configuration;

public sealed class AppOptions
{
    public StorageOptions Storage { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
    public OrdersOptions Orders { get; set; } = new();
    public string EnvironmentName { get; set; } = "Development";
}

public sealed class StorageOptions
{
    public string DataPath { get; set; } = "data";
    public string FileName { get; set; } = "orders.json";

    public string ResolveFullPath(string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            throw new ArgumentException(
                "Базовий каталог не може бути порожнім",
                nameof(baseDirectory));
        }

        if (string.IsNullOrWhiteSpace(DataPath))
        {
            throw new ArgumentException("Storage:DataPath не може бути порожнім");
        }

        if (string.IsNullOrWhiteSpace(FileName))
        {
            throw new ArgumentException("Storage:FileName не може бути порожнім");
        }

        string root = Path.IsPathRooted(DataPath)
            ? DataPath
            : Path.Combine(baseDirectory, DataPath);
        string fullPath = Path.GetFullPath(Path.Combine(root, FileName));

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        return fullPath;
    }
}

public sealed class LoggingOptions
{
    public string MinimumLevel { get; set; } = "Information";
}

public sealed class OrdersOptions
{
    public string Currency { get; set; } = "UAH";
}
