using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Configuration;

public class TtsProviderOptionsBindsCorrectlyFromConfigurationTest : TestBase
{
    public TtsProviderOptionsBindsCorrectlyFromConfigurationTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["TtsProvider:Provider"] = "ElevenLabs",
            ["TtsProvider:ApiKeySecretName"] = "tts-key",
            ["TtsProvider:VoiceId"] = "voice-123"
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<TtsProviderOptions>(config.GetSection(TtsProviderOptions.SectionName));

        var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<TtsProviderOptions>>().Value;

        // Assert
        Verify(options);
    }
}
