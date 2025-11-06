using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YetAnotherTranslator.Infrastructure.Persistence.Entities;

[Table("llm_response_cache")]
public class LlmResponseCacheEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("cache_key")]
    [MaxLength(64)]
    public string CacheKey { get; set; } = string.Empty;

    [Required]
    [Column("operation_type")]
    [MaxLength(50)]
    public string OperationType { get; set; } = string.Empty;

    [Required]
    [Column("request_hash")]
    [MaxLength(64)]
    public string RequestHash { get; set; } = string.Empty;

    [Required]
    [Column("response_json", TypeName = "jsonb")]
    public string ResponseJson { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }
}
