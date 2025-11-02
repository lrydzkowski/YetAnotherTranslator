using Microsoft.EntityFrameworkCore;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence;

public class TranslatorDbContext : DbContext
{
    public TranslatorDbContext(DbContextOptions<TranslatorDbContext> options)
        : base(options)
    {
    }

    public DbSet<HistoryEntryEntity> HistoryEntries => Set<HistoryEntryEntity>();
    public DbSet<TranslationCacheEntity> TranslationCache => Set<TranslationCacheEntity>();
    public DbSet<TextTranslationCacheEntity> TextTranslationCache => Set<TextTranslationCacheEntity>();
    public DbSet<PronunciationCacheEntity> PronunciationCache => Set<PronunciationCacheEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HistoryEntryEntity>(entity =>
        {
            entity.ToTable("history_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.CommandType).IsRequired().HasConversion<string>();
            entity.Property(e => e.InputText).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.OutputText).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.LlmMetadata).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.CommandType);
        });

        modelBuilder.Entity<TranslationCacheEntity>(entity =>
        {
            entity.ToTable("translation_cache");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CacheKey).IsRequired().HasMaxLength(64);
            entity.Property(e => e.SourceWord).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SourceLanguage).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TargetLanguage).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ResultJson).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.AccessedAt).IsRequired();
            entity.Property(e => e.AccessCount).IsRequired().HasDefaultValue(1);

            entity.HasIndex(e => e.CacheKey).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<TextTranslationCacheEntity>(entity =>
        {
            entity.ToTable("text_translation_cache");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CacheKey).IsRequired().HasMaxLength(64);
            entity.Property(e => e.SourceTextHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.SourceLanguage).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TargetLanguage).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ResultJson).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.AccessedAt).IsRequired();
            entity.Property(e => e.AccessCount).IsRequired().HasDefaultValue(1);

            entity.HasIndex(e => e.CacheKey).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<PronunciationCacheEntity>(entity =>
        {
            entity.ToTable("pronunciation_cache");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CacheKey).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PartOfSpeech).HasMaxLength(50);
            entity.Property(e => e.VoiceId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AudioData).IsRequired();
            entity.Property(e => e.AudioFormat).IsRequired().HasMaxLength(10);
            entity.Property(e => e.AudioSizeBytes).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.AccessedAt).IsRequired();
            entity.Property(e => e.AccessCount).IsRequired().HasDefaultValue(1);

            entity.HasIndex(e => e.CacheKey).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
