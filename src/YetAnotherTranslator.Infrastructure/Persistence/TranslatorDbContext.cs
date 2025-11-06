using Microsoft.EntityFrameworkCore;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence;

public class TranslatorDbContext : DbContext
{
    public TranslatorDbContext(DbContextOptions<TranslatorDbContext> options)
        : base(options)
    {
    }

    public DbSet<HistoryEntryEntity> HistoryEntries { get; set; } = null!;
    public DbSet<TranslationCacheEntity> TranslationCache { get; set; } = null!;
    public DbSet<TextTranslationCacheEntity> TextTranslationCache { get; set; } = null!;
    public DbSet<PronunciationCacheEntity> PronunciationCache { get; set; } = null!;
    public DbSet<LlmResponseCacheEntity> LlmResponseCache { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HistoryEntryEntity>(
            entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CreatedAt);
            }
        );

        modelBuilder.Entity<TranslationCacheEntity>(
            entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CacheKey).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
            }
        );

        modelBuilder.Entity<TextTranslationCacheEntity>(
            entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CacheKey).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
            }
        );

        modelBuilder.Entity<PronunciationCacheEntity>(
            entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CacheKey).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
            }
        );

        modelBuilder.Entity<LlmResponseCacheEntity>(
            entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CacheKey).IsUnique();
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => e.CreatedAt);
            }
        );
    }
}
