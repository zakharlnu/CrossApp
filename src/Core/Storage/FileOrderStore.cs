using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileOrderStore : IOrderStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Dictionary<string, Order> _cache =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path;
    private bool _loaded;

    public FileOrderStore(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Шлях до сховища не може бути порожнім",
                nameof(path));
        }

        _path = Path.GetFullPath(path);
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Order? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureLoaded();

        if (_cache.ContainsKey(order.Id))
        {
            throw new DomainException(
                $"Замовлення з id={order.Id} уже існує");
        }

        _cache.Add(order.Id, order);
        Flush();
    }

    public void Update(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureLoaded();
        _cache[order.Id] = order;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_cache.Remove(id))
        {
            return false;
        }

        Flush();
        return true;
    }

    public void ReplaceAll(IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        var replacement = new Dictionary<string, Order>(
            StringComparer.OrdinalIgnoreCase);

        foreach (Order order in orders)
        {
            if (!replacement.TryAdd(order.Id, order))
            {
                throw new DomainException(
                    $"Замовлення з id={order.Id} дублюється в колекції");
            }
        }

        _cache.Clear();

        foreach ((string id, Order order) in replacement)
        {
            _cache.Add(id, order);
        }

        _loaded = true;
        Flush();
    }

    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);
            List<OrderDto> dtos =
                JsonSerializer.Deserialize<List<OrderDto>>(json, Options) ?? [];

            _cache.Clear();

            foreach (OrderDto dto in dtos)
            {
                Order order = Order.FromDto(dto);
                _cache[order.Id] = order;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        string directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);

        List<OrderDto> dtos = _cache.Values
            .OrderBy(order => order.Id, StringComparer.Ordinal)
            .Select(order => order.ToDto())
            .ToList();
        string json = JsonSerializer.Serialize(dtos, Options);
        string temporaryPath = _path + ".tmp";

        try
        {
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, _path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
