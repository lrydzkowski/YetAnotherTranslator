namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

internal class CacheEntity
{
    public Guid Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string? ResultJson { get; set; }
    public byte[]? ResultByte { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
