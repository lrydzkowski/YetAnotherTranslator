using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public record TranslateWordRequest(
    string Word,
    SourceLanguage SourceLanguage,
    string TargetLanguage,
    bool UseCache = true
);
