using YetAnotherTranslator.Core.Handlers.TranslateText.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces;

public interface ICacheRepository
{
    Task<TextTranslationResult?> GetTextTranslationAsync(
        string text,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    );

    Task SaveTextTranslationAsync(
        TextTranslationResult result,
        CancellationToken cancellationToken = default
    );
}
