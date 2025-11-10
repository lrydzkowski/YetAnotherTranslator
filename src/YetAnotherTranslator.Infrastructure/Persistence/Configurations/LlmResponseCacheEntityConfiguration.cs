using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence.Configurations;

public class LlmResponseCacheEntityConfiguration : IEntityTypeConfiguration<LlmResponseCacheEntity>
{
    public void Configure(EntityTypeBuilder<LlmResponseCacheEntity> builder)
    {
        builder.ToTable("llm_response_cache");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.CacheKey)
            .HasColumnName("cache_key")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.OperationType)
            .HasColumnName("operation_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.RequestHash)
            .HasColumnName("request_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.ResponseJson)
            .HasColumnName("response_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired(false);

        builder.HasIndex(e => e.CacheKey)
            .IsUnique();

        builder.HasIndex(e => e.ExpiresAt);

        builder.HasIndex(e => e.CreatedAt);
    }
}
