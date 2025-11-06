namespace YetAnotherTranslator.Infrastructure.Configuration;

public class TtsProviderConfiguration
{
    public string Provider { get; set; } = string.Empty;
    public string ApiKeySecretName { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;
}
