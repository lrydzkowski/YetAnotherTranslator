using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence.Configurations;

internal class PronunciationCacheEntityConfiguration : IEntityTypeConfiguration<PronunciationCacheEntity>
{
    public void Configure(EntityTypeBuilder<PronunciationCacheEntity> builder)
    {
        builder.ToTable("pronunciation_cache");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.CacheKey)
            .HasColumnName("cache_key")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.Text)
            .HasColumnName("text")
            .HasMaxLength(6000)
            .IsRequired();

        builder.Property(e => e.PartOfSpeech)
            .HasColumnName("part_of_speech")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(e => e.AudioData)
            .HasColumnName("audio_data")
            .IsRequired();

        builder.Property(e => e.VoiceId)
            .HasColumnName("voice_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(e => e.CacheKey)
            .IsUnique();

        builder.HasIndex(e => e.CreatedAt);
    }
}
