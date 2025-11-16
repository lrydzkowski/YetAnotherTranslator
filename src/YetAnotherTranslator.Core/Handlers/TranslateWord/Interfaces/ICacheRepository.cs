using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces;

public interface ICacheRepository
{
    Task<TranslationResult?> GetTranslationAsync(
        string word,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    );

    Task SaveTranslationAsync(
        TranslationResult result,
        CancellationToken cancellationToken = default
    );
}
