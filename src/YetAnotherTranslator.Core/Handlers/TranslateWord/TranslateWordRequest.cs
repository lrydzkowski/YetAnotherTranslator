namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public record TranslateWordRequest(
    string Word,
    string SourceLanguage,
    string TargetLanguage,
    bool UseCache = true
);
