using FluentValidation;
using Microsoft.Extensions.Logging;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces;
using YetAnotherTranslator.Core.Handlers.TranslateText.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateText;

public interface ITranslateTextHandler
{
    Task<TextTranslationResult> HandleAsync(
        TranslateTextRequest request,
        CancellationToken cancellationToken = default
    );
}

internal class TranslateTextHandler : ITranslateTextHandler
{
    private readonly ICacheRepository _cacheRepository;
    private readonly IHistoryRepository _historyRepository;
    private readonly ILargeLanguageModelProvider _largeLanguageModelProvider;
    private readonly ILogger<TranslateTextHandler> _logger;
    private readonly ISerializer _serializer;
    private readonly IValidator<TranslateTextRequest> _validator;

    public TranslateTextHandler(
        ILargeLanguageModelProvider largeLanguageModelProvider,
        IValidator<TranslateTextRequest> validator,
        IHistoryRepository historyRepository,
        ICacheRepository cacheRepository,
        ISerializer serializer,
        ILogger<TranslateTextHandler> logger
    )
    {
        _largeLanguageModelProvider = largeLanguageModelProvider;
        _validator = validator;
        _historyRepository = historyRepository;
        _cacheRepository = cacheRepository;
        _serializer = serializer;
        _logger = logger;
    }

    public async Task<TextTranslationResult> HandleAsync(
        TranslateTextRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);
            if (request.UseCache)
            {
                TextTranslationResult? cached = await GetTextTranslationFromCacheAsync(request, cancellationToken);
                if (cached is not null)
                {
                    return cached;
                }
            }

            string translatedText = await TranslateTextAsync(request, cancellationToken);
            TextTranslationResult result = new()
            {
                SourceLanguage = request.SourceLanguage,
                TargetLanguage = request.TargetLanguage,
                InputText = request.Text,
                TranslatedText = translatedText
            };
            await SaveCacheAsync(result, cancellationToken);
            await SaveHistoryAsync(request, result, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error translating text. Text length: {TextLength}, SourceLanguage: {SourceLanguage}, TargetLanguage: {TargetLanguage}",
                request.Text.Length,
                request.SourceLanguage,
                request.TargetLanguage
            );

            throw;
        }
    }

    private async Task<TextTranslationResult?> GetTextTranslationFromCacheAsync(
        TranslateTextRequest request,
        CancellationToken cancellationToken
    )
    {
        TextTranslationResult? cached = await _cacheRepository.GetTextTranslationAsync(
            request.Text,
            request.SourceLanguage,
            request.TargetLanguage,
            cancellationToken
        );

        return cached;
    }

    private async Task<string> TranslateTextAsync(TranslateTextRequest request, CancellationToken cancellationToken)
    {
        string translatedText = await _largeLanguageModelProvider.TranslateTextAsync(
            request.Text,
            request.SourceLanguage,
            request.TargetLanguage,
            cancellationToken
        );

        return translatedText;
    }

    private async Task SaveCacheAsync(
        TextTranslationResult result,
        CancellationToken cancellationToken
    )
    {
        await _cacheRepository.SaveTextTranslationAsync(result, cancellationToken);
    }

    private async Task SaveHistoryAsync(
        TranslateTextRequest request,
        TextTranslationResult result,
        CancellationToken cancellationToken
    )
    {
        await _historyRepository.SaveHistoryAsync(
            request.CommandType,
            request.Text,
            _serializer.Serialize(result),
            cancellationToken
        );
    }
}
