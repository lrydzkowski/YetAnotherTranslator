namespace YetAnotherTranslator.Infrastructure.Configuration;

public class ApplicationConfiguration
{
    public SecretManagerConfiguration SecretManager { get; set; } = new();
    public LlmProviderConfiguration LlmProvider { get; set; } = new();
    public TtsProviderConfiguration TtsProvider { get; set; } = new();
    public DatabaseConfiguration Database { get; set; } = new();
}
