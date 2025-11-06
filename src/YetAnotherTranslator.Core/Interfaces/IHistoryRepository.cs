namespace YetAnotherTranslator.Core.Interfaces;

public interface IHistoryRepository
{
    Task SaveHistoryAsync<T>(
        string commandType,
        string inputText,
        T result,
        CancellationToken cancellationToken = default
    )
        where T : class;

    Task<List<HistoryEntry>> GetHistoryAsync(int limit = 100, CancellationToken cancellationToken = default);

    Task<T?> GetCachedTranslationAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        where T : class;

    Task SaveCachedTranslationAsync<T>(string cacheKey, T result, CancellationToken cancellationToken = default)
        where T : class;
}

public class HistoryEntry
{
    public Guid Id { get; set; }
    public string CommandType { get; set; } = string.Empty;
    public string InputText { get; set; } = string.Empty;
    public string OutputText { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}
