using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class LlmProviderOptionsBindsCorrectlyFromConfigurationTest : TestBase
{
    public LlmProviderOptionsBindsCorrectlyFromConfigurationTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        var configData = new Dictionary<string, string?>
        {
            ["LlmProvider:Provider"] = "Anthropic",
            ["LlmProvider:Model"] = "claude-3-5-sonnet-20241022",
            ["LlmProvider:ApiKeySecretName"] = "test-key",
            ["LlmProvider:MaxTokens"] = "2048",
            ["LlmProvider:Temperature"] = "0.5"
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<LlmProviderOptions>(config.GetSection(LlmProviderOptions.SectionName));

        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<LlmProviderOptions>>().Value;

        Verify(options);
    }
}
