using Testcontainers.PostgreSql;
using WireMock.Server;

namespace YetAnotherTranslator.Tests.Integration.Infrastructure;

public class IntegrationTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgresContainer { get; private set; } = null!;
    public WireMockServer WireMockServer { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        PostgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("translator_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await PostgresContainer.StartAsync();

        WireMockServer = WireMockServer.Start();
    }

    public async Task DisposeAsync()
    {
        WireMockServer?.Stop();
        WireMockServer?.Dispose();

        if (PostgresContainer != null)
        {
            await PostgresContainer.DisposeAsync();
        }
    }
}
