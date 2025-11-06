using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WireMock.Server;
using YetAnotherTranslator.Infrastructure.Persistence;

namespace YetAnotherTranslator.Tests.Integration.Infrastructure;

public abstract class TestBase : IAsyncLifetime
{
    protected PostgreSqlContainer PostgresContainer { get; private set; } = null!;
    protected WireMockServer WireMockServer { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected TranslatorDbContext DbContext { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        PostgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("translator_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await PostgresContainer.StartAsync();

        WireMockServer = WireMockServer.Start();

        ServiceCollection services = new();
        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<TranslatorDbContext>();

        await DbContext.Database.MigrateAsync();
    }

    public virtual async Task DisposeAsync()
    {
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

        WireMockServer?.Stop();
        WireMockServer?.Dispose();

        if (PostgresContainer != null)
        {
            await PostgresContainer.DisposeAsync();
        }
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TranslatorDbContext>(
            options =>
                options.UseNpgsql(PostgresContainer.GetConnectionString())
        );
    }
}
