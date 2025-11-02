using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using YetAnotherTranslator.Core.Interfaces;
using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Infrastructure.Llm;

public class AnthropicProvider : ILlmProvider
{
    private readonly AnthropicClient _client;
    private const string ModelId = AnthropicModels.Claude35Sonnet;

    public AnthropicProvider(string apiKey)
    {
        _client = new AnthropicClient(apiKey);
    }

    public async Task<TranslationResult> TranslateWordAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"You are a professional translator and linguist. Translate the word ""{word}"" from {sourceLanguage} to {targetLanguage}.

IMPORTANT: Provide translations ordered by most common usage (rank 1 = most common). Include:
- Up to 3 translations maximum
- Part of speech for each translation (noun, verb, adjective, etc.)
- For nouns, specify countability: ""countable"", ""uncountable"", or ""both""
- 2-3 example sentences for each translation showing natural usage in {targetLanguage}

Return ONLY a JSON object with this exact structure (no markdown, no explanation):
{{
  ""sourceWord"": ""{word}"",
  ""sourceLanguage"": ""{sourceLanguage}"",
  ""targetLanguage"": ""{targetLanguage}"",
  ""detectedLanguage"": ""{sourceLanguage}"",
  ""translations"": [
    {{
      ""rank"": 1,
      ""word"": ""most common translation"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      ""examples"": [""Example sentence 1"", ""Example sentence 2""]
    }}
  ]
}}";

        var messages = new List<Message>
        {
            new Message(RoleType.User, prompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = 2048,
            Model = ModelId,
            Stream = false,
            Temperature = 0.3m
        };

        var response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);
        var textContent = response.Content.OfType<TextContent>().FirstOrDefault();

        if (textContent == null)
        {
            throw new InvalidOperationException("No text response from Claude");
        }

        var content = textContent.Text;
        var result = JsonSerializer.Deserialize<TranslationResult>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new InvalidOperationException("Failed to parse translation result");
    }

    public Task<TextTranslationResult> TranslateTextAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GrammarReviewResult> ReviewGrammarAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string> DetectLanguageAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
