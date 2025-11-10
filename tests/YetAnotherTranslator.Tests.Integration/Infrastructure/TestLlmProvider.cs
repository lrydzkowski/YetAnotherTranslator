using System.Net;
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
        return await TestConnectionAsync(cancellationToken);
    }

    public async Task<string> TranslateWordAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        return await TestConnectionAsync(cancellationToken);
    }

    public async Task<string> TranslateTextAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        return await TestConnectionAsync(cancellationToken);
    }

    public async Task<string> ReviewGrammarAsync(string text, CancellationToken cancellationToken = default)
    {
        return await TestConnectionAsync(cancellationToken);
    }

    private async Task<string> TestConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/v1/messages",
                cancellationToken
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
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
