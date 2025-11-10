using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WireMock.Server;
using YetAnotherTranslator.Infrastructure.Persistence;

namespace YetAnotherTranslator.Tests.Integration.Infrastructure;

[Collection("IntegrationTests")]
public abstract class TestBase : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    protected PostgreSqlContainer PostgresContainer => _fixture.PostgresContainer;
    protected WireMockServer WireMockServer => _fixture.WireMockServer;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected TranslatorDbContext DbContext { get; private set; } = null!;

    protected TestBase(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        ServiceCollection services = new();
        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<TranslatorDbContext>();

        await DbContext.Database.MigrateAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await CleanupDatabaseAsync();

        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }

        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        WireMockServer.Reset();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TranslatorDbContext>(
            options =>
                options.UseNpgsql(PostgresContainer.GetConnectionString())
        );

        services.AddScoped<YetAnotherTranslator.Core.Interfaces.IHistoryRepository,
            YetAnotherTranslator.Infrastructure.Persistence.HistoryRepository>();
    }

    private async Task CleanupDatabaseAsync()
    {
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE history_entries CASCADE");
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE translation_cache CASCADE");
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE text_translation_cache CASCADE");
        await DbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE pronunciation_cache CASCADE");
    }
}
