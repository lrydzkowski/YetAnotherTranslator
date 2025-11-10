using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.TranslateWord;

namespace YetAnotherTranslator.Core.Interfaces;

public interface IHistoryRepository
{
    Task<TranslationResult?> GetCachedTranslationAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default
    );

    Task SaveTranslationAsync(
        TranslationResult result,
        CancellationToken cancellationToken = default
    );

    Task SaveHistoryAsync(
        CommandType commandType,
        string inputText,
        string outputText,
        CancellationToken cancellationToken = default
    );

    Task<List<HistoryEntry>> GetHistoryAsync(
        int limit = 50,
        CancellationToken cancellationToken = default
    );
}
