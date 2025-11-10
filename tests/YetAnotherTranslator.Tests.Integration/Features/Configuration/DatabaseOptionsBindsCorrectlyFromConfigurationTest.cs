using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class DatabaseOptionsBindsCorrectlyFromConfigurationTest : TestBase
{
    public DatabaseOptionsBindsCorrectlyFromConfigurationTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Database:ConnectionStringSecretName"] = "db-connection"
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<DatabaseOptions>(config.GetSection(DatabaseOptions.SectionName));

        var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        // Assert
        Verify(options);
    }
}
