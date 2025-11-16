using FluentValidation;
using YetAnotherTranslator.Core.Common.Models;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces;
using YetAnotherTranslator.Core.Handlers.TranslateText.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateText;

public class TranslateTextHandler
{
    private readonly ICacheRepository _cacheRepository;
    private readonly IHistoryRepository _historyRepository;
    private readonly ILargeLanguageModelProvider _largeLanguageModelProvider;
    private readonly ISerializer _serializer;
    private readonly IValidator<TranslateTextRequest> _validator;

    public TranslateTextHandler(
        ILargeLanguageModelProvider largeLanguageModelProvider,
        IValidator<TranslateTextRequest> validator,
        IHistoryRepository historyRepository,
        ICacheRepository cacheRepository,
        ISerializer serializer
    )
    {
        _largeLanguageModelProvider = largeLanguageModelProvider;
        _validator = validator;
        _historyRepository = historyRepository;
        _cacheRepository = cacheRepository;
        _serializer = serializer;
    }

    public async Task<TextTranslationResult> HandleAsync(
        TranslateTextRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        if (request.UseCache)
        {
            TextTranslationResult? cached = await _cacheRepository.GetTextTranslationAsync(
                request.Text,
                request.SourceLanguage,
                request.TargetLanguage,
                cancellationToken
            );

            if (cached is not null)
            {
                return cached;
            }
        }

        string translatedText = await _largeLanguageModelProvider.TranslateTextAsync(
            request.Text,
            request.SourceLanguage,
            request.TargetLanguage,
            cancellationToken
        );
        TextTranslationResult result = new()
        {
            SourceLanguage = request.SourceLanguage,
            TargetLanguage = request.TargetLanguage,
            InputText = request.Text,
            TranslatedText = translatedText
        };

        await _cacheRepository.SaveTextTranslationAsync(result, cancellationToken);
        await _historyRepository.SaveHistoryAsync(
            CommandType.TranslateText,
            request.Text,
            _serializer.Serialize(result),
            cancellationToken
        );

        return result;
    }
}
