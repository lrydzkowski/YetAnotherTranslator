using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace YetAnotherTranslator.Infrastructure.Persistence;

internal class TranslatorDbContextFactory : IDesignTimeDbContextFactory<TranslatorDbContext>
{
    public TranslatorDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<TranslatorDbContext> builder = GetDbContextOptionsBuilder();

        return new TranslatorDbContext(builder.Options);
    }

    private DbContextOptionsBuilder<TranslatorDbContext> GetDbContextOptionsBuilder()
    {
        string connectionString = GetConnectionString();
        DbContextOptionsBuilder<TranslatorDbContext> builder = new();
        builder.UseNpgsql(
            connectionString,
            x => x.MigrationsAssembly(typeof(TranslatorDbContext).Assembly.FullName)
        );

        return builder;
    }

    private string GetConnectionString()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddUserSecrets<TranslatorDbContext>()
            .Build();
        string connectionString = DatabaseOptions.GetConnectionString(configuration);

        return connectionString;
    }
}
