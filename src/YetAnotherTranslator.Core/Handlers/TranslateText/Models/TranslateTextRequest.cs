namespace YetAnotherTranslator.Core.Handlers.TranslateText.Models;

public record TranslateTextRequest(
    string Text,
    string? SourceLanguage,
    string? TargetLanguage,
    bool UseCache = true
);
