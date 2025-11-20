using FluentValidation;
using YetAnotherTranslator.Core.Common.Models;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Interfaces;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public class TranslateWordHandler
{
    private readonly ICacheRepository _cacheRepository;
    private readonly IHistoryRepository _historyRepository;
    private readonly ILargeLanguageModelProvider _largeLanguageModelProvider;
    private readonly ISerializer _serializer;
    private readonly IValidator<TranslateWordRequest> _validator;

    public TranslateWordHandler(
        ILargeLanguageModelProvider largeLanguageModelProvider,
        IValidator<TranslateWordRequest> validator,
        ICacheRepository cacheRepository,
        IHistoryRepository historyRepository,
        ISerializer serializer
    )
    {
        _largeLanguageModelProvider = largeLanguageModelProvider;
        _validator = validator;
        _cacheRepository = cacheRepository;
        _historyRepository = historyRepository;
        _serializer = serializer;
    }

    public async Task<TranslationResult> HandleAsync(
        TranslateWordRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        if (request.UseCache)
        {
            TranslationResult? cached = await _cacheRepository.GetTranslationAsync(
                request.Word,
                request.SourceLanguage,
                request.TargetLanguage,
                cancellationToken
            );
            if (cached is not null)
            {
                return cached;
            }
        }

        TranslationResult? result = await _largeLanguageModelProvider.TranslateWordAsync(
                request.Word,
                request.SourceLanguage,
                request.TargetLanguage,
                cancellationToken
            )
            ?? new TranslationResult();
        result.SourceLanguage = request.SourceLanguage;
        result.TargetLanguage = request.TargetLanguage;
        result.InputText = request.Word;

        await _cacheRepository.SaveTranslationAsync(result, cancellationToken);
        await _historyRepository.SaveHistoryAsync(
            CommandType.TranslateWord,
            request.Word,
            _serializer.Serialize(result),
            cancellationToken
        );

        return result;
    }
}
