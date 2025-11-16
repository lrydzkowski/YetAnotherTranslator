namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation.Models;

public class PronunciationResult
{
    public string Text { get; init; } = string.Empty;
    public string? PartOfSpeech { get; init; }
    public bool Played { get; init; }
}
