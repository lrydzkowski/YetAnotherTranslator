namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

internal class HistoryEntryEntity
{
    public Guid Id { get; set; }
    public string CommandType { get; set; } = string.Empty;
    public string InputText { get; set; } = string.Empty;
    public string? OutputText { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
