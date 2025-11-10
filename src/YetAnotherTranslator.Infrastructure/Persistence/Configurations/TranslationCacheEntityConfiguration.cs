using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence.Configurations;

public class TranslationCacheEntityConfiguration : IEntityTypeConfiguration<TranslationCacheEntity>
{
    public void Configure(EntityTypeBuilder<TranslationCacheEntity> builder)
    {
        builder.ToTable("translation_cache");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.CacheKey)
            .HasColumnName("cache_key")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.SourceLanguage)
            .HasColumnName("source_language")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TargetLanguage)
            .HasColumnName("target_language")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.InputText)
            .HasColumnName("input_text")
            .IsRequired();

        builder.Property(e => e.ResultJson)
            .HasColumnName("result_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.CacheKey)
            .IsUnique();

        builder.HasIndex(e => e.CreatedAt);
    }
}
