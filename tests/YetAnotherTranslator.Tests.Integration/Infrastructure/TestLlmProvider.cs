using System.Net;
using System.Text;
using System.Text.Json;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Tests.Integration.Infrastructure;

public class TestLlmProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public TestLlmProvider(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public async Task<string> DetectLanguageAsync(string text, CancellationToken cancellationToken = default)
    {
        return await CallApiAsync(new { text }, cancellationToken);
    }

    public async Task<string> TranslateWordAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        return await CallApiAsync(new { word, sourceLanguage, targetLanguage }, cancellationToken);
    }

    public async Task<string> TranslateTextAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        return await CallApiAsync(new { text, sourceLanguage, targetLanguage }, cancellationToken);
    }

    public async Task<string> ReviewGrammarAsync(string text, CancellationToken cancellationToken = default)
    {
        return await CallApiAsync(new { text }, cancellationToken);
    }

    private async Task<string> CallApiAsync(object requestData, CancellationToken cancellationToken)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/v1/messages",
                content,
                cancellationToken
            );

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new ExternalServiceException(
                    "Anthropic",
                    "Failed to connect to Anthropic API: Service Unavailable"
                );
            }

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var responseDoc = JsonDocument.Parse(responseBody);
            if (responseDoc.RootElement.TryGetProperty("content", out var contentArray) &&
                contentArray.GetArrayLength() > 0)
            {
                var firstContent = contentArray[0];
                if (firstContent.TryGetProperty("text", out var textProp))
                {
                    return textProp.GetString() ?? string.Empty;
                }
            }

            return responseBody;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new ExternalServiceException(
                "Anthropic",
                "Connection to Anthropic API timed out. Please check your network connection",
                ex
            );
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException(
                "Anthropic",
                $"Failed to connect to Anthropic API: {ex.Message}",
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
                $"Unexpected error connecting to LLM provider: {ex.Message}",
                ex
            );
        }
    }
}
