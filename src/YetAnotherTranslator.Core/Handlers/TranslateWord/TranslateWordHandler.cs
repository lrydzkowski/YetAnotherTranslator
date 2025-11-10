using System.Text.Json;
using FluentValidation;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public class TranslateWordHandler
{
    private readonly ILlmProvider _llmProvider;
    private readonly IValidator<TranslateWordRequest> _validator;
    private readonly IHistoryRepository _historyRepository;

    public TranslateWordHandler(
        ILlmProvider llmProvider,
        IValidator<TranslateWordRequest> validator,
        IHistoryRepository historyRepository)
    {
        _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    public async Task<TranslationResult> HandleAsync(
        TranslateWordRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        if (request.UseCache)
        {
            TranslationResult? cached = await _historyRepository.GetCachedTranslationAsync(
                request.Word,
                request.SourceLanguage,
                request.TargetLanguage,
                cancellationToken
            );

            if (cached != null)
            {
                return cached;
            }
        }

        string llmResponse = await _llmProvider.TranslateWordAsync(
            request.Word,
            request.SourceLanguage,
            request.TargetLanguage,
            cancellationToken
        );

        TranslationResult result = ParseLlmResponse(llmResponse, request);

        await _historyRepository.SaveTranslationAsync(result, cancellationToken);

        await _historyRepository.SaveHistoryAsync(
            CommandType.TranslateWord,
            request.Word,
            JsonSerializer.Serialize(result),
            cancellationToken
        );

        return result;
    }

    private static TranslationResult ParseLlmResponse(string llmResponse, TranslateWordRequest request)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(llmResponse);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("translations", out JsonElement translationsElement))
            {
                throw new ExternalServiceException("Anthropic", "Invalid LLM response: missing 'translations' field");
            }

            var translations = new List<Translation>();

            foreach (JsonElement transElement in translationsElement.EnumerateArray())
            {
                var translation = new Translation
                {
                    Rank = transElement.GetProperty("rank").GetInt32(),
                    Word = transElement.GetProperty("word").GetString() ?? string.Empty,
                    PartOfSpeech = transElement.GetProperty("partOfSpeech").GetString() ?? string.Empty,
                    Countability = transElement.TryGetProperty("countability", out JsonElement countElement)
                        ? countElement.GetString()
                        : null,
                    CmuArpabet = transElement.TryGetProperty("cmuArpabet", out JsonElement arpaElement) &&
                                 arpaElement.ValueKind != JsonValueKind.Null
                        ? arpaElement.GetString()
                        : null,
                    Examples = transElement.TryGetProperty("examples", out JsonElement examplesElement)
                        ? examplesElement.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList()
                        : new List<string>()
                };

                translations.Add(translation);
            }

            return new TranslationResult
            {
                SourceLanguage = request.SourceLanguage,
                TargetLanguage = request.TargetLanguage,
                InputText = request.Word,
                Translations = translations
            };
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException(
                "Anthropic",
                $"Failed to parse LLM response: {ex.Message}. Response excerpt: {llmResponse.Substring(0, Math.Min(200, llmResponse.Length))}",
                ex
            );
        }
    }
}
