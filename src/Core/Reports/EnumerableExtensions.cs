namespace Core.Reports;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TopBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        Func<T, string> tieBreakerSelector,
        int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(tieBreakerSelector);

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                count,
                "Кількість елементів не може бути від'ємною");
        }

        return source
            .OrderByDescending(keySelector)
            .ThenBy(tieBreakerSelector, StringComparer.Ordinal)
            .Take(count);
    }
}
