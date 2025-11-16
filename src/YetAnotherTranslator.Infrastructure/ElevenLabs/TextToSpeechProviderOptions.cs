namespace YetAnotherTranslator.Infrastructure.ElevenLabs;

internal class TextToSpeechProviderOptions
{
    public const string Position = "TextToSpeechProvider";

    public string Provider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;
}
