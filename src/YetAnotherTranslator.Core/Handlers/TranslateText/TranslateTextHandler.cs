using System.Text.Json;
using FluentValidation;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Interfaces;
using YetAnotherTranslator.Core.Models;
using ValidationException = FluentValidation.ValidationException;

namespace YetAnotherTranslator.Core.Handlers.TranslateText;

public class TranslateTextHandler
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ILlmProvider _llmProvider;
    private readonly IValidator<TranslateTextRequest> _validator;

    public TranslateTextHandler(
        ILlmProvider llmProvider,
        IValidator<TranslateTextRequest> validator,
        IHistoryRepository historyRepository
    )
    {
        _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    public async Task<TextTranslationResult> HandleAsync(
        TranslateTextRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        string sourceLanguage = await DetermineSourceLanguageAsync(request, cancellationToken);

        if (request.UseCache)
        {
            TextTranslationResult? cached = await _historyRepository.GetCachedTextTranslationAsync(
                request.Text,
                sourceLanguage,
                request.TargetLanguage,
                cancellationToken
            );

            if (cached != null)
            {
                return cached;
            }
        }

        string translatedText = await _llmProvider.TranslateTextAsync(
            request.Text,
            sourceLanguage,
            request.TargetLanguage,
            cancellationToken
        );

        TextTranslationResult result = new()
        {
            SourceLanguage = sourceLanguage,
            TargetLanguage = request.TargetLanguage,
            InputText = request.Text,
            TranslatedText = translatedText
        };

        await _historyRepository.SaveTextTranslationAsync(result, cancellationToken);

        await _historyRepository.SaveHistoryAsync(
            CommandType.TranslateText,
            request.Text,
            JsonSerializer.Serialize(result),
            cancellationToken
        );

        return result;
    }

    private async Task<string> DetermineSourceLanguageAsync(
        TranslateTextRequest request,
        CancellationToken cancellationToken
    )
    {
        if (request.SourceLanguage == SourceLanguage.Polish)
        {
            return "Polish";
        }

        if (request.SourceLanguage == SourceLanguage.English)
        {
            return "English";
        }

        string detectionResponse = await _llmProvider.DetectLanguageAsync(request.Text, cancellationToken);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(detectionResponse);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("language", out JsonElement languageElement))
            {
                throw new ExternalServiceException(
                    "Anthropic",
                    "Invalid language detection response: missing 'language' field"
                );
            }

            if (!root.TryGetProperty("confidence", out JsonElement confidenceElement))
            {
                throw new ExternalServiceException(
                    "Anthropic",
                    "Invalid language detection response: missing 'confidence' field"
                );
            }

            string? detectedLanguage = languageElement.GetString();
            int confidence = confidenceElement.GetInt32();

            if (confidence < 80)
            {
                throw new ValidationException(
                    $"Language detection confidence too low: {confidence}%. Please specify the source language explicitly."
                );
            }

            if (detectedLanguage != "Polish" && detectedLanguage != "English")
            {
                throw new ValidationException(
                    $"Detected language '{detectedLanguage}' is not supported. Only Polish and English are supported."
                );
            }

            return detectedLanguage;
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException(
                "Anthropic",
                $"Failed to parse language detection response: {ex.Message}",
                ex
            );
        }
    }
}
