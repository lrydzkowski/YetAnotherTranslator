using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

[Table("pronunciation_cache")]
public class PronunciationCacheEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("cache_key")]
    [MaxLength(64)]
    public string CacheKey { get; set; } = string.Empty;

    [Required]
    [Column("text")]
    public string Text { get; set; } = string.Empty;

    [Column("part_of_speech")]
    [MaxLength(50)]
    public string? PartOfSpeech { get; set; }

    [Required]
    [Column("audio_data")]
    public byte[] AudioData { get; set; } = [];

    [Required]
    [Column("voice_id")]
    [MaxLength(100)]
    public string VoiceId { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
