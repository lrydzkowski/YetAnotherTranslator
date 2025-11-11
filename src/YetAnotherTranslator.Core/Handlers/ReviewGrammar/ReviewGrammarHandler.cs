using System.Text.Json;
using FluentValidation;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Interfaces;
using ValidationException = YetAnotherTranslator.Core.Exceptions.ValidationException;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

public class ReviewGrammarHandler
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ILlmProvider _llmProvider;
    private readonly IValidator<ReviewGrammarRequest> _validator;

    public ReviewGrammarHandler(
        ILlmProvider llmProvider,
        IValidator<ReviewGrammarRequest> validator,
        IHistoryRepository historyRepository
    )
    {
        _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    public async Task<GrammarReviewResult> HandleAsync(
        ReviewGrammarRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        await VerifyEnglishTextAsync(request.Text, cancellationToken);

        string llmResponse = await _llmProvider.ReviewGrammarAsync(
            request.Text,
            cancellationToken
        );

        GrammarReviewResult result = ParseLlmResponse(llmResponse, request);

        await _historyRepository.SaveHistoryAsync(
            CommandType.ReviewGrammar,
            request.Text,
            JsonSerializer.Serialize(result),
            cancellationToken
        );

        return result;
    }

    private async Task VerifyEnglishTextAsync(string text, CancellationToken cancellationToken)
    {
        string detectionResponse = await _llmProvider.DetectLanguageAsync(text, cancellationToken);

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

            string? detectedLanguage = languageElement.GetString();

            if (detectedLanguage != "English")
            {
                throw new ValidationException(
                    $"Grammar review only supports English text. Detected language: {detectedLanguage}"
                );
            }
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

    private static GrammarReviewResult ParseLlmResponse(string llmResponse, ReviewGrammarRequest request)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(llmResponse);
            JsonElement root = doc.RootElement;

            List<GrammarIssue> grammarIssues = new();
            List<VocabularySuggestion> vocabularySuggestions = new();

            if (root.TryGetProperty("grammarIssues", out JsonElement grammarElement))
            {
                foreach (JsonElement issueElement in grammarElement.EnumerateArray())
                {
                    GrammarIssue issue = new()
                    {
                        Issue = issueElement.GetProperty("issue").GetString() ?? string.Empty,
                        Correction = issueElement.GetProperty("correction").GetString() ?? string.Empty,
                        Explanation = issueElement.GetProperty("explanation").GetString() ?? string.Empty
                    };
                    grammarIssues.Add(issue);
                }
            }

            if (root.TryGetProperty("vocabularySuggestions", out JsonElement vocabElement))
            {
                foreach (JsonElement suggestionElement in vocabElement.EnumerateArray())
                {
                    VocabularySuggestion suggestion = new()
                    {
                        Original = suggestionElement.GetProperty("original").GetString() ?? string.Empty,
                        Suggestion = suggestionElement.GetProperty("suggestion").GetString() ?? string.Empty,
                        Context = suggestionElement.GetProperty("context").GetString() ?? string.Empty
                    };
                    vocabularySuggestions.Add(suggestion);
                }
            }

            return new GrammarReviewResult
            {
                InputText = request.Text,
                GrammarIssues = grammarIssues,
                VocabularySuggestions = vocabularySuggestions
            };
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException(
                "Anthropic",
                $"Failed to parse grammar review response: {ex.Message}. Response excerpt: {llmResponse.Substring(0, Math.Min(200, llmResponse.Length))}",
                ex
            );
        }
    }
}
