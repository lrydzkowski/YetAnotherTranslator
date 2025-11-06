using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

[Table("translation_cache")]
public class TranslationCacheEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("cache_key")]
    [MaxLength(64)]
    public string CacheKey { get; set; } = string.Empty;

    [Required]
    [Column("source_language")]
    [MaxLength(50)]
    public string SourceLanguage { get; set; } = string.Empty;

    [Required]
    [Column("target_language")]
    [MaxLength(50)]
    public string TargetLanguage { get; set; } = string.Empty;

    [Required]
    [Column("input_text")]
    public string InputText { get; set; } = string.Empty;

    [Required]
    [Column("result_json", TypeName = "jsonb")]
    public string ResultJson { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
