namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

public class TextTranslationCacheEntity
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string SourceLanguage { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
    public string InputText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
