using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YetAnotherTranslator.Infrastructure.Persistence;

public class TranslatorDbContextFactory : IDesignTimeDbContextFactory<TranslatorDbContext>
{
    public TranslatorDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<TranslatorDbContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql("Host=localhost;Database=translator_dev;Username=postgres;Password=postgres");

        return new TranslatorDbContext(optionsBuilder.Options);
    }
}
