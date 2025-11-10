using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateText;

public record TranslateTextRequest(
    string Text,
    SourceLanguage SourceLanguage,
    string TargetLanguage,
    bool UseCache = true
);
