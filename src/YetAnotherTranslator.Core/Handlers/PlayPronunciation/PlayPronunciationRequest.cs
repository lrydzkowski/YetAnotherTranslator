namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation;

public record PlayPronunciationRequest(
    string Text,
    string? PartOfSpeech = null,
    bool UseCache = true
);
