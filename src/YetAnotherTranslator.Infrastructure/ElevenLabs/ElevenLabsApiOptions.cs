namespace YetAnotherTranslator.Infrastructure.ElevenLabs;

internal class ElevenLabsApiOptions
{
    public const string Position = "ElevenLabsApi";

    public string ApiKey { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;
}
