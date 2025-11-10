namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

public class LlmResponseCacheEntity
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
