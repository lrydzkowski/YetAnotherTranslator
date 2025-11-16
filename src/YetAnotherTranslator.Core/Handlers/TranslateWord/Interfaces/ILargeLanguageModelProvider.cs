using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces;

public interface ILargeLanguageModelProvider
{
    Task<List<Translation>> TranslateWordAsync(
        string word,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    );
}
