namespace YetAnotherTranslator.Infrastructure.Configuration;

public class LlmProviderConfiguration
{
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ApiKeySecretName { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public double Temperature { get; set; }
}
