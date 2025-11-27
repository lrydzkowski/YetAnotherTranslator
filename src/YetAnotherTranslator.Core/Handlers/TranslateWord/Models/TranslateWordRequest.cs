using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

public record TranslateWordRequest(
    CommandType CommandType,
    string Word,
    string? SourceLanguage,
    string? TargetLanguage,
    bool UseCache = true
);
