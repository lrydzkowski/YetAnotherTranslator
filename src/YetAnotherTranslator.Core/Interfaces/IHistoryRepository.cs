using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Core.Interfaces;

public interface IHistoryRepository
{
    Task SaveHistoryEntryAsync(
        CommandType commandType,
        string inputText,
        string outputJson,
        string? llmMetadataJson,
        CancellationToken cancellationToken = default);

    Task<List<HistoryEntry>> GetHistoryAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);

    Task<TranslationResult?> GetTranslationCacheAsync(
        string cacheKey,
        CancellationToken cancellationToken = default);

    Task SaveTranslationCacheAsync(
        string cacheKey,
        string sourceWord,
        string sourceLanguage,
        string targetLanguage,
        TranslationResult result,
        CancellationToken cancellationToken = default);

    Task<TextTranslationResult?> GetTextTranslationCacheAsync(
        string cacheKey,
        CancellationToken cancellationToken = default);

    Task SaveTextTranslationCacheAsync(
        string cacheKey,
        string sourceTextHash,
        string sourceLanguage,
        string targetLanguage,
        TextTranslationResult result,
        CancellationToken cancellationToken = default);

    Task<PronunciationResult?> GetPronunciationCacheAsync(
        string cacheKey,
        CancellationToken cancellationToken = default);

    Task SavePronunciationCacheAsync(
        string cacheKey,
        string text,
        string? partOfSpeech,
        string voiceId,
        PronunciationResult result,
        CancellationToken cancellationToken = default);
}
