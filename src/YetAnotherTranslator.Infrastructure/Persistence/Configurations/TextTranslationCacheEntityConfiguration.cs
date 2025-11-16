using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence.Configurations;

internal class TextTranslationCacheEntityConfiguration : IEntityTypeConfiguration<TextTranslationCacheEntity>
{
    public void Configure(EntityTypeBuilder<TextTranslationCacheEntity> builder)
    {
        builder.ToTable("text_translation_cache");

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
            .HasMaxLength(6000)
            .IsRequired();

        builder.Property(e => e.TranslatedText)
            .HasColumnName("translated_text")
            .HasMaxLength(6000)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.CacheKey)
            .IsUnique();

        builder.HasIndex(e => e.CreatedAt);
    }
}
