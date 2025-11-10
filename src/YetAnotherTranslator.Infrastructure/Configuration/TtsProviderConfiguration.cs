namespace YetAnotherTranslator.Infrastructure.Configuration;

public class TtsProviderOptions
{
    public const string SectionName = "TtsProvider";

    public string Provider { get; set; } = string.Empty;
    public string ApiKeySecretName { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;
}
