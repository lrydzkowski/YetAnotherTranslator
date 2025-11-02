namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

public class PronunciationCacheEntity
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? PartOfSpeech { get; set; }
    public string VoiceId { get; set; } = string.Empty;
    public byte[] AudioData { get; set; } = Array.Empty<byte>();
    public string AudioFormat { get; set; } = string.Empty;
    public int AudioSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime AccessedAt { get; set; }
    public int AccessCount { get; set; } = 1;
}
