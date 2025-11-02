using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YetAnotherTranslator.Core.Commands;
using YetAnotherTranslator.Core.Interfaces;
using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Core.Handlers;

public class TranslateWordHandler
{
    private readonly ILlmProvider _llmProvider;
    private readonly IHistoryRepository _historyRepository;

    public TranslateWordHandler(ILlmProvider llmProvider, IHistoryRepository historyRepository)
    {
        _llmProvider = llmProvider;
        _historyRepository = historyRepository;
    }

    public async Task<TranslationResult> HandleAsync(
        TranslateWordCommand command,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey(command.Word, command.SourceLanguage, command.TargetLanguage);

        var cachedResult = await _historyRepository.GetTranslationCacheAsync(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var result = await _llmProvider.TranslateWordAsync(
            command.Word,
            command.SourceLanguage,
            command.TargetLanguage,
            cancellationToken);

        await _historyRepository.SaveTranslationCacheAsync(
            cacheKey,
            command.Word,
            command.SourceLanguage,
            command.TargetLanguage,
            result,
            cancellationToken);

        await _historyRepository.SaveHistoryEntryAsync(
            CommandType.TranslateWord,
            command.Word,
            JsonSerializer.Serialize(result),
            null,
            cancellationToken);

        return result;
    }

    private static string GenerateCacheKey(string word, string sourceLanguage, string targetLanguage)
    {
        var input = $"{word.ToLowerInvariant()}|{sourceLanguage.ToLowerInvariant()}|{targetLanguage.ToLowerInvariant()}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
