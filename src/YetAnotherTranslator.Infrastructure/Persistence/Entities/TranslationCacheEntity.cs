namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

internal class TranslationCacheEntity
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string? SourceLanguage { get; set; } = string.Empty;
    public string? TargetLanguage { get; set; } = string.Empty;
    public string InputText { get; set; } = string.Empty;
    public string ResultJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
