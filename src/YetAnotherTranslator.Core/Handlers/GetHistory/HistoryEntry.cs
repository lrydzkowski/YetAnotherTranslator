namespace YetAnotherTranslator.Core.Handlers.GetHistory;

public class HistoryEntry
{
    public CommandType CommandType { get; init; }
    public string InputText { get; init; } = string.Empty;
    public string OutputText { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
