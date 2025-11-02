namespace YetAnotherTranslator.Core.Models;

public record HistoryEntry(
    Guid Id,
    DateTime Timestamp,
    CommandType CommandType,
    string InputText,
    string OutputText,
    string? LlmMetadata,
    DateTime CreatedAt);
