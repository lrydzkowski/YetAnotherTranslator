using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

[Table("history_entries")]
public class HistoryEntryEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("command_type")]
    [MaxLength(50)]
    public string CommandType { get; set; } = string.Empty;

    [Required]
    [Column("input_text")]
    public string InputText { get; set; } = string.Empty;

    [Required]
    [Column("output_text")]
    public string OutputText { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
