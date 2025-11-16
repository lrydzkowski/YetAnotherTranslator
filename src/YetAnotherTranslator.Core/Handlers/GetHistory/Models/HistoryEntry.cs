using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Core.Handlers.GetHistory.Models;

public class HistoryEntry
{
    public CommandType CommandType { get; init; }
    public string InputText { get; init; } = string.Empty;
    public string? OutputText { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
