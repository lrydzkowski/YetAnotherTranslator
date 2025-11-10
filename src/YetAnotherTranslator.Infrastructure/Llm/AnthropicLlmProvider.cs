using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Infrastructure.Llm;

public class AnthropicLlmProvider : ILlmProvider
{
    private readonly AnthropicClient _client;
    private readonly string _model;
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysMs = { 0, 1000, 2000 };

    public AnthropicLlmProvider(string apiKey, string model = "claude-sonnet-4-20250514")
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ConfigurationException("Anthropic API key cannot be empty");
        }

        _client = new AnthropicClient(new APIAuthentication(apiKey));
        _model = model;
    }

    public async Task<string> DetectLanguageAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty", nameof(text));
        }

        string systemPrompt = "You are a language detection expert. Detect if the given text is in Polish or English and return a confidence score.";

        string userPrompt = $@"Detect the language of the following text and return confidence score:

Text: {text}

Return response as JSON with this exact structure:
{{
  ""language"": ""Polish"" or ""English"",
  ""confidence"": 85
}}

Only return the JSON, no additional text.";

        try
        {
            var parameters = new MessageParameters
            {
                Messages = new List<Message>
                {
                    new Message(RoleType.User, userPrompt)
                },
                MaxTokens = 100,
                Model = _model,
                Stream = false,
                Temperature = 0.2m,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
            };

            MessageResponse response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            if (response.Content == null || response.Content.Count == 0)
            {
                throw new ExternalServiceException("Anthropic", "Empty response from LLM");
            }

            var textContent = response.Content[0] as Anthropic.SDK.Messaging.TextContent;
            string content = textContent?.Text ?? string.Empty;
            return content;
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException(
                "Anthropic",
                $"Failed to detect language: {ex.Message}",
                ex
            );
        }
    }

    public async Task<string> TranslateWordAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            throw new ArgumentException("Word cannot be empty", nameof(word));
        }

        bool includeArpabet = sourceLanguage == "Polish" && targetLanguage == "English";

        string systemPrompt = @"You are a professional translator specializing in Polish-English translation.
You provide detailed linguistic information including CMU Arpabet phonetic transcriptions for English words.";

        string cmuArpabetInstruction = includeArpabet
            ? @"   - CMU Arpabet phonetic transcription using standard CMU dictionary format
3. If a word has pronunciation variants by part of speech (e.g., ""record"" as noun vs verb),
   provide separate CMU Arpabet for each variant"
            : string.Empty;

        string cmuArpabetField = includeArpabet
            ? @"""cmuArpabet"": ""K AE1 T"" or null,"
            : string.Empty;

        string userPrompt = $@"Translate the {sourceLanguage} word '{word}' to {targetLanguage}.

Provide:
1. Multiple translations ranked by popularity (most common first)
2. For each translation:
   - The {targetLanguage} word
   - Part of speech (noun, verb, adjective, etc.)
   - Countability (countable/uncountable for nouns, N/A otherwise)
{cmuArpabetInstruction}
   - Example sentence demonstrating usage

Return response as structured JSON with schema:
{{
  ""translations"": [
    {{
      ""rank"": 1,
      ""word"": ""cat"",
      ""partOfSpeech"": ""noun"",
      ""countability"": ""countable"",
      {cmuArpabetField}
      ""examples"": [""The cat sat on the mat.""]
    }}
  ]
}}

{(includeArpabet ? "If CMU Arpabet cannot be generated for a word, use null for the cmuArpabet field." : "")}
Only return the JSON, no additional text.";

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    await Task.Delay(RetryDelaysMs[attempt], cancellationToken);
                }

                var parameters = new MessageParameters
                {
                    Messages = new List<Message>
                    {
                        new Message(RoleType.User, userPrompt)
                    },
                    MaxTokens = 2048,
                    Model = _model,
                    Stream = false,
                    Temperature = 0.3m,
                    System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
                };

                MessageResponse response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

                if (response.Content == null || response.Content.Count == 0)
                {
                    throw new ExternalServiceException("Anthropic", "Empty response from LLM");
                }

                var textContent = response.Content[0] as Anthropic.SDK.Messaging.TextContent;
                string content = textContent?.Text ?? string.Empty;

                ValidateJsonResponse(content);

                return content;
            }
            catch (JsonException) when (attempt < MaxRetries - 1)
            {
                continue;
            }
            catch (JsonException ex)
            {
                throw new ExternalServiceException(
                    "Anthropic",
                    $"Failed to parse LLM response after {MaxRetries} attempts. Last error: {ex.Message}",
                    ex
                );
            }
            catch (ExternalServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ExternalServiceException(
                    "Anthropic",
                    $"Failed to translate word: {ex.Message}",
                    ex
                );
            }
        }

        throw new ExternalServiceException("Anthropic", $"Failed to translate word after {MaxRetries} attempts");
    }

    public async Task<string> TranslateTextAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty", nameof(text));
        }

        string systemPrompt = $"You are a professional translator specializing in {sourceLanguage}-{targetLanguage} translation.";

        string userPrompt = $@"Translate the following {sourceLanguage} text to {targetLanguage}:

{text}

Preserve paragraph structure and formatting. Return only the translated text, no additional commentary.";

        try
        {
            var parameters = new MessageParameters
            {
                Messages = new List<Message>
                {
                    new Message(RoleType.User, userPrompt)
                },
                MaxTokens = 4096,
                Model = _model,
                Stream = false,
                Temperature = 0.3m,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
            };

            MessageResponse response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            if (response.Content == null || response.Content.Count == 0)
            {
                throw new ExternalServiceException("Anthropic", "Empty response from LLM");
            }

            var textContent = response.Content[0] as Anthropic.SDK.Messaging.TextContent;
            return textContent?.Text ?? string.Empty;
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException(
                "Anthropic",
                $"Failed to translate text: {ex.Message}",
                ex
            );
        }
    }

    public async Task<string> ReviewGrammarAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty", nameof(text));
        }

        string systemPrompt = @"You are an English grammar expert. Analyze text for grammar errors and vocabulary improvements.
Focus on: subject-verb agreement, article usage, tense errors, double negatives, and plural forms.";

        string userPrompt = $@"Review the following English text for grammar and vocabulary:

{text}

Provide:
1. Grammar issues with corrections and explanations
2. Vocabulary suggestions for improvement

Return response as structured JSON:
{{
  ""grammarIssues"": [
    {{
      ""issue"": ""Subject-verb disagreement"",
      ""correction"": ""The cat sits (not 'sit')"",
      ""explanation"": ""Singular subject requires singular verb""
    }}
  ],
  ""vocabularySuggestions"": [
    {{
      ""original"": ""good"",
      ""suggestion"": ""excellent"",
      ""context"": ""More impactful in formal writing""
    }}
  ]
}}

Only return the JSON, no additional text.";

        try
        {
            var parameters = new MessageParameters
            {
                Messages = new List<Message>
                {
                    new Message(RoleType.User, userPrompt)
                },
                MaxTokens = 4096,
                Model = _model,
                Stream = false,
                Temperature = 0.5m,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
            };

            MessageResponse response = await _client.Messages.GetClaudeMessageAsync(parameters, cancellationToken);

            if (response.Content == null || response.Content.Count == 0)
            {
                throw new ExternalServiceException("Anthropic", "Empty response from LLM");
            }

            var textContent = response.Content[0] as Anthropic.SDK.Messaging.TextContent;
            return textContent?.Text ?? string.Empty;
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException(
                "Anthropic",
                $"Failed to review grammar: {ex.Message}",
                ex
            );
        }
    }

    private static void ValidateJsonResponse(string content)
    {
        JsonDocument.Parse(content);
    }
}
