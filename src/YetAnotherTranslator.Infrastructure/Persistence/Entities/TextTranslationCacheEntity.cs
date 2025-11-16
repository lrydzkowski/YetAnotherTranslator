namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

internal class TextTranslationCacheEntity
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string? SourceLanguage { get; set; }
    public string? TargetLanguage { get; set; }
    public string InputText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
