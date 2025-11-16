namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

internal class PronunciationCacheEntity
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? PartOfSpeech { get; set; }
    public byte[] AudioData { get; set; } = [];
    public string VoiceId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
