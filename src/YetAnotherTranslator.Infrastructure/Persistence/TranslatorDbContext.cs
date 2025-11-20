using Microsoft.EntityFrameworkCore;
using YetAnotherTranslator.Infrastructure.Persistence.Entities;

namespace YetAnotherTranslator.Infrastructure.Persistence;

internal class TranslatorDbContext : DbContext
{
    public TranslatorDbContext(DbContextOptions<TranslatorDbContext> options)
        : base(options)
    {
    }

    public DbSet<HistoryEntryEntity> HistoryEntries { get; set; } = null!;
    public DbSet<CacheEntity> Cache { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TranslatorDbContext).Assembly);
    }
}
