using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

public class HistoryEntryEntity
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public CommandType CommandType { get; set; }
    public string InputText { get; set; } = string.Empty;
    public string OutputText { get; set; } = string.Empty;
    public string? LlmMetadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
