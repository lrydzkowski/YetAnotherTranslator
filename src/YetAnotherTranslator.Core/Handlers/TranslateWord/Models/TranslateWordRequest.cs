namespace YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

public record TranslateWordRequest(
    string Word,
    string? SourceLanguage,
    string? TargetLanguage,
    bool UseCache = true
);
