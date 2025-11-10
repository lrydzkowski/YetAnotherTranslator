using FluentAssertions;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Infrastructure.Configuration;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class ConfigurationValidationTests : TestBase
{
    private readonly string _testConfigDirectory;
    private readonly string _testConfigPath;

    public ConfigurationValidationTests()
    {
        _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"translator_test_{Guid.NewGuid()}");
        _testConfigPath = Path.Combine(_testConfigDirectory, "config.json");
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Directory.CreateDirectory(_testConfigDirectory);
        Environment.SetEnvironmentVariable("APPDATA", Path.GetTempPath());
    }

    public override async Task DisposeAsync()
    {
        if (Directory.Exists(_testConfigDirectory))
        {
            Directory.Delete(_testConfigDirectory, true);
        }

        await base.DisposeAsync();
    }

    [Fact]
    public async Task LoadConfiguration_MissingConfigFile_ThrowsConfigurationException()
    {
        var loader = new ConfigurationLoader(_testConfigPath);

        Func<Task> act = async () => await loader.LoadConfigurationAsync();

        await act.Should()
            .ThrowAsync<ConfigurationException>()
            .WithMessage("Configuration file not found at:*");
    }

    [Fact]
    public async Task LoadConfiguration_MalformedJson_ThrowsConfigurationExceptionWithLineAndColumn()
    {
        string malformedJson = @"{
  ""SecretManager"": {
    ""Provider"": ""AzureKeyVault""
    ""KeyVaultUrl"": ""https://test.vault.azure.net""
  }
}";
        await File.WriteAllTextAsync(_testConfigPath, malformedJson);

        var loader = new ConfigurationLoader(_testConfigPath);

        Func<Task> act = async () => await loader.LoadConfigurationAsync();

        await act.Should()
            .ThrowAsync<ConfigurationException>()
            .Where(
                ex => ex.Message.Contains("Malformed JSON")
                    && ex.Message.Contains("Line:")
                    && ex.Message.Contains("Position:")
            );
    }

    [Fact]
    public async Task LoadConfiguration_MissingLlmProvider_ThrowsConfigurationException()
    {
        string incompleteConfig = @"{
  ""SecretManager"": {
    ""Provider"": ""AzureKeyVault"",
    ""KeyVaultUrl"": ""https://test.vault.azure.net""
  },
  ""TtsProvider"": {
    ""Provider"": ""ElevenLabs"",
    ""ApiKeySecretName"": ""elevenlabs-api-key"",
    ""VoiceId"": ""test-voice-id""
  },
  ""Database"": {
    ""ConnectionString"": ""Host=localhost;Database=test;Username=test;Password=test""
  }
}";
        await File.WriteAllTextAsync(_testConfigPath, incompleteConfig);

        var loader = new ConfigurationLoader(_testConfigPath);

        Func<Task> act = async () => await loader.LoadConfigurationAsync();

        await act.Should()
            .ThrowAsync<ConfigurationException>()
            .WithMessage("*LlmProvider*required*");
    }

    [Fact]
    public async Task LoadConfiguration_InvalidKeyVaultUrl_ThrowsConfigurationException()
    {
        string invalidUrlConfig = @"{
  ""SecretManager"": {
    ""Provider"": ""AzureKeyVault"",
    ""KeyVaultUrl"": ""http://invalid-url""
  },
  ""LlmProvider"": {
    ""Provider"": ""Anthropic"",
    ""Model"": ""claude-sonnet-4.5"",
    ""ApiKeySecretName"": ""anthropic-api-key"",
    ""MaxTokens"": 2048,
    ""Temperature"": 0.3
  },
  ""TtsProvider"": {
    ""Provider"": ""ElevenLabs"",
    ""ApiKeySecretName"": ""elevenlabs-api-key"",
    ""VoiceId"": ""test-voice-id""
  },
  ""Database"": {
    ""ConnectionString"": ""Host=localhost;Database=test;Username=test;Password=test""
  }
}";
        await File.WriteAllTextAsync(_testConfigPath, invalidUrlConfig);

        var loader = new ConfigurationLoader(_testConfigPath);

        Func<Task> act = async () => await loader.LoadConfigurationAsync();

        await act.Should()
            .ThrowAsync<ConfigurationException>()
            .WithMessage("*KeyVaultUrl*HTTPS*");
    }

    [Fact]
    public async Task LoadConfiguration_ValidConfig_LoadsSuccessfully()
    {
        string validConfig = @"{
  ""SecretManager"": {
    ""Provider"": ""AzureKeyVault"",
    ""KeyVaultUrl"": ""https://test.vault.azure.net""
  },
  ""LlmProvider"": {
    ""Provider"": ""Anthropic"",
    ""Model"": ""claude-sonnet-4.5"",
    ""ApiKeySecretName"": ""anthropic-api-key"",
    ""MaxTokens"": 2048,
    ""Temperature"": 0.3
  },
  ""TtsProvider"": {
    ""Provider"": ""ElevenLabs"",
    ""ApiKeySecretName"": ""elevenlabs-api-key"",
    ""VoiceId"": ""test-voice-id""
  },
  ""Database"": {
    ""ConnectionString"": ""Host=localhost;Database=test;Username=test;Password=test""
  }
}";
        await File.WriteAllTextAsync(_testConfigPath, validConfig);

        var loader = new ConfigurationLoader(_testConfigPath);

        ApplicationConfiguration config = await loader.LoadConfigurationAsync();

        config.Should().NotBeNull();
        config.SecretManager.Provider.Should().Be("AzureKeyVault");
        config.SecretManager.KeyVaultUrl.Should().Be("https://test.vault.azure.net");
        config.LlmProvider.Provider.Should().Be("Anthropic");
        config.LlmProvider.Model.Should().Be("claude-sonnet-4.5");
        config.LlmProvider.MaxTokens.Should().Be(2048);
        config.LlmProvider.Temperature.Should().Be(0.3);
        config.TtsProvider.Provider.Should().Be("ElevenLabs");
        config.TtsProvider.VoiceId.Should().Be("test-voice-id");
        config.Database.ConnectionString.Should().Be("Host=localhost;Database=test;Username=test;Password=test");
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
