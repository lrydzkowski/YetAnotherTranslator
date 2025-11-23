using FluentValidation;
using Microsoft.Extensions.Logging;
using YetAnotherTranslator.Core.Common.Models;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public interface ITranslateWordHandler
{
    Task<TranslationResult> HandleAsync(
        TranslateWordRequest request,
        CancellationToken cancellationToken = default
    );
}

internal class TranslateWordHandler : ITranslateWordHandler
{
    private readonly ICacheRepository _cacheRepository;
    private readonly IHistoryRepository _historyRepository;
    private readonly ILargeLanguageModelProvider _largeLanguageModelProvider;
    private readonly ILogger<TranslateWordHandler> _logger;
    private readonly ISerializer _serializer;
    private readonly IValidator<TranslateWordRequest> _validator;

    public TranslateWordHandler(
        ILargeLanguageModelProvider largeLanguageModelProvider,
        IValidator<TranslateWordRequest> validator,
        ICacheRepository cacheRepository,
        IHistoryRepository historyRepository,
        ISerializer serializer,
        ILogger<TranslateWordHandler> logger
    )
    {
        _largeLanguageModelProvider = largeLanguageModelProvider;
        _validator = validator;
        _cacheRepository = cacheRepository;
        _historyRepository = historyRepository;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task<TranslationResult> HandleAsync(
        TranslateWordRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);
            if (request.UseCache)
            {
                TranslationResult? cached = await GetTranslationFromCacheAsync(request, cancellationToken);
                if (cached is not null)
                {
                    return cached;
                }
            }

            TranslationResult? result = await TranslateWordAsync(request, cancellationToken);
            result.SourceLanguage = request.SourceLanguage;
            result.TargetLanguage = request.TargetLanguage;
            result.InputText = request.Word;

            await SaveCacheAsync(result, cancellationToken);
            await SaveHistoryAsync(request, result, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error translating word. Word: {Word}, SourceLanguage: {SourceLanguage}, TargetLanguage: {TargetLanguage}",
                request.Word,
                request.SourceLanguage,
                request.TargetLanguage
            );

            throw;
        }
    }

    private async Task<TranslationResult?> GetTranslationFromCacheAsync(
        TranslateWordRequest request,
        CancellationToken cancellationToken
    )
    {
        TranslationResult? cached = await _cacheRepository.GetTranslationAsync(
            request.Word,
            request.SourceLanguage,
            request.TargetLanguage,
            cancellationToken
        );

        return cached;
    }

    private async Task<TranslationResult> TranslateWordAsync(
        TranslateWordRequest request,
        CancellationToken cancellationToken
    )
    {
        TranslationResult result = await _largeLanguageModelProvider.TranslateWordAsync(
                request.Word,
                request.SourceLanguage,
                request.TargetLanguage,
                cancellationToken
            )
            ?? new TranslationResult();

        return result;
    }

    private async Task SaveCacheAsync(
        TranslationResult result,
        CancellationToken cancellationToken
    )
    {
        await _cacheRepository.SaveTranslationAsync(result, cancellationToken);
    }

    private async Task SaveHistoryAsync(
        TranslateWordRequest request,
        TranslationResult result,
        CancellationToken cancellationToken
    )
    {
        await _historyRepository.SaveHistoryAsync(
            CommandType.TranslateWord,
            request.Word,
            _serializer.Serialize(result),
            cancellationToken
        );
    }
}
