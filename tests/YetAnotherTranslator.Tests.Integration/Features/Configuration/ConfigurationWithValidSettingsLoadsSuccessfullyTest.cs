using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class ConfigurationWithValidSettingsLoadsSuccessfullyTest : TestBase
{
    public ConfigurationWithValidSettingsLoadsSuccessfullyTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        var configData = new Dictionary<string, string?>
        {
            ["KeyVault:VaultName"] = "test-vault",
            ["LlmProvider:Provider"] = "Anthropic",
            ["LlmProvider:Model"] = "claude-3-5-sonnet-20241022",
            ["LlmProvider:ApiKeySecretName"] = "anthropic-api-key",
            ["LlmProvider:MaxTokens"] = "4096",
            ["LlmProvider:Temperature"] = "0.3",
            ["TtsProvider:Provider"] = "ElevenLabs",
            ["TtsProvider:ApiKeySecretName"] = "elevenlabs-api-key",
            ["TtsProvider:VoiceId"] = "test-voice-id",
            ["Database:ConnectionStringSecretName"] = "postgres-connection-string"
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<KeyVaultOptions>(config.GetSection(KeyVaultOptions.SectionName));
        services.Configure<LlmProviderOptions>(config.GetSection(LlmProviderOptions.SectionName));
        services.Configure<TtsProviderOptions>(config.GetSection(TtsProviderOptions.SectionName));
        services.Configure<DatabaseOptions>(config.GetSection(DatabaseOptions.SectionName));

        var provider = services.BuildServiceProvider();

        var keyVaultOptions = provider.GetRequiredService<IOptions<KeyVaultOptions>>().Value;
        var llmOptions = provider.GetRequiredService<IOptions<LlmProviderOptions>>().Value;
        var ttsOptions = provider.GetRequiredService<IOptions<TtsProviderOptions>>().Value;
        var dbOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        Verify(new
        {
            KeyVaultOptions = keyVaultOptions,
            LlmOptions = llmOptions,
            TtsOptions = ttsOptions,
            DbOptions = dbOptions
        });
    }
}
