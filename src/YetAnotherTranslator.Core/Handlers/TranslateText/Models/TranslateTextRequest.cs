using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateText.Models;

public record TranslateTextRequest(
    CommandType CommandType,
    string Text,
    string? SourceLanguage,
    string? TargetLanguage,
    bool UseCache = true
);
