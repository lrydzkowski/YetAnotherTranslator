namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

public class HistoryEntryEntity
{
    public Guid Id { get; set; }
    public string CommandType { get; set; } = string.Empty;
    public string InputText { get; set; } = string.Empty;
    public string OutputText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
