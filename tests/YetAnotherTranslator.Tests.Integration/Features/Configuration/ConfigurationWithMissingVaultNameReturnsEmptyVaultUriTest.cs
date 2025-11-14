using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class ConfigurationWithMissingVaultNameReturnsEmptyVaultUriTest : TestBase
{
    public ConfigurationWithMissingVaultNameReturnsEmptyVaultUriTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        var configData = new Dictionary<string, string?>
        {
            ["KeyVault:VaultName"] = ""
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<KeyVaultOptions>(config.GetSection(KeyVaultOptions.SectionName));

        var provider = services.BuildServiceProvider();

        var keyVaultOptions = provider.GetRequiredService<IOptions<KeyVaultOptions>>().Value;

        Verify(keyVaultOptions);
    }
}
