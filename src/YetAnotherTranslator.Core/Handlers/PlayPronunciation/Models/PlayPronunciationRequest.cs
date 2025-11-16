namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation.Models;

public record PlayPronunciationRequest(
    string Text,
    string? PartOfSpeech = null,
    bool UseCache = true
);
