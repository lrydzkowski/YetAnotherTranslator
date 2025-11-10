using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class ConfigurationValidationTests : TestBase
{
    public ConfigurationValidationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Configuration_WithValidSettings_LoadsSuccessfully()
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

        keyVaultOptions.VaultName.Should().Be("test-vault");
        keyVaultOptions.VaultUri.Should().Be("https://test-vault.vault.azure.net");
        llmOptions.Provider.Should().Be("Anthropic");
        llmOptions.Model.Should().Be("claude-3-5-sonnet-20241022");
        llmOptions.MaxTokens.Should().Be(4096);
        llmOptions.Temperature.Should().Be(0.3);
        ttsOptions.Provider.Should().Be("ElevenLabs");
        ttsOptions.VoiceId.Should().Be("test-voice-id");
        dbOptions.ConnectionStringSecretName.Should().Be("postgres-connection-string");
    }

    [Fact]
    public void Configuration_WithMissingVaultName_ReturnsEmptyVaultUri()
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

        keyVaultOptions.VaultUri.Should().BeEmpty();
    }

    [Fact]
    public void LlmProviderOptions_BindsCorrectlyFromConfiguration()
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

        options.Provider.Should().Be("Anthropic");
        options.Model.Should().Be("claude-3-5-sonnet-20241022");
        options.ApiKeySecretName.Should().Be("test-key");
        options.MaxTokens.Should().Be(2048);
        options.Temperature.Should().Be(0.5);
    }

    [Fact]
    public void TtsProviderOptions_BindsCorrectlyFromConfiguration()
    {
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
        var options = provider.GetRequiredService<IOptions<TtsProviderOptions>>().Value;

        options.Provider.Should().Be("ElevenLabs");
        options.ApiKeySecretName.Should().Be("tts-key");
        options.VoiceId.Should().Be("voice-123");
    }

    [Fact]
    public void DatabaseOptions_BindsCorrectlyFromConfiguration()
    {
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
        var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        options.ConnectionStringSecretName.Should().Be("db-connection");
    }

    [Fact]
    public async Task SecretsProvider_NetworkTimeout_ThrowsExternalServiceException()
    {
        WireMockServer.Given(
            WireMock.RequestBuilders.Request
                .Create()
                .WithPath("/secrets/test-key")
                .UsingGet()
        )
        .RespondWith(
            WireMock.ResponseBuilders.Response
                .Create()
                .WithDelay(TimeSpan.FromSeconds(10))
                .WithStatusCode(200)
        );

        var provider = new Infrastructure.TestSecretsProvider(WireMockServer.Url!);

        Func<Task> act = async () => await provider.GetSecretAsync("test-key");

        await act.Should()
            .ThrowAsync<ExternalServiceException>()
            .Where(ex =>
                ex.ServiceName == "AzureKeyVault" &&
                ex.Message.Contains("timed out")
            );
    }

    [Fact]
    public async Task SecretsProvider_PermissionDenied_ThrowsExternalServiceException()
    {
        WireMockServer.Given(
            WireMock.RequestBuilders.Request
                .Create()
                .WithPath("/secrets/test-key")
                .UsingGet()
        )
        .RespondWith(
            WireMock.ResponseBuilders.Response
                .Create()
                .WithStatusCode(403)
        );

        var provider = new Infrastructure.TestSecretsProvider(WireMockServer.Url!);

        Func<Task> act = async () => await provider.GetSecretAsync("test-key");

        await act.Should()
            .ThrowAsync<ExternalServiceException>()
            .Where(ex =>
                ex.ServiceName == "AzureKeyVault" &&
                ex.Message.Contains("Access denied") &&
                ex.Message.Contains("az login")
            );
    }

    [Fact]
    public async Task SecretsProvider_SecretNotFound_ThrowsExternalServiceException()
    {
        WireMockServer.Given(
            WireMock.RequestBuilders.Request
                .Create()
                .WithPath("/secrets/nonexistent-key")
                .UsingGet()
        )
        .RespondWith(
            WireMock.ResponseBuilders.Response
                .Create()
                .WithStatusCode(404)
        );

        var provider = new Infrastructure.TestSecretsProvider(WireMockServer.Url!);

        Func<Task> act = async () => await provider.GetSecretAsync("nonexistent-key");

        await act.Should()
            .ThrowAsync<ExternalServiceException>()
            .Where(ex =>
                ex.ServiceName == "AzureKeyVault" &&
                ex.Message.Contains("not found")
            );
    }

    [Fact]
    public async Task LlmProvider_ConnectionFailure_ThrowsExternalServiceException()
    {
        WireMockServer.Given(
            WireMock.RequestBuilders.Request
                .Create()
                .WithPath("/v1/messages")
                .UsingGet()
        )
        .RespondWith(
            WireMock.ResponseBuilders.Response
                .Create()
                .WithStatusCode(503)
        );

        var provider = new Infrastructure.TestLlmProvider(WireMockServer.Url!);

        Func<Task> act = async () => await provider.DetectLanguageAsync("test");

        await act.Should()
            .ThrowAsync<ExternalServiceException>()
            .Where(ex =>
                ex.ServiceName == "Anthropic" &&
                ex.Message.Contains("Failed to connect")
            );
    }
}
