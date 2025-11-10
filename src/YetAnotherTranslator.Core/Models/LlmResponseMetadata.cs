namespace YetAnotherTranslator.Core.Models;

public class LlmResponseMetadata
{
    public int InputTokens { get; init; }
    public int OutputTokens { get; init; }
    public string Model { get; init; } = string.Empty;
    public string ResponseId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
