using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YetAnotherTranslator.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TranslatorDbContext>
{
    public TranslatorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TranslatorDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Database=translator_design;Username=postgres;Password=postgres");

        return new TranslatorDbContext(optionsBuilder.Options);
    }
}
