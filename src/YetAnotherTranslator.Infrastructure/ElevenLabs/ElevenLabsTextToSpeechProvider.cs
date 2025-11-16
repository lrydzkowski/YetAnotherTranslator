using System.Text;
using System.Text.Json;
using YetAnotherTranslator.Core.Common.Exceptions;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;

namespace YetAnotherTranslator.Infrastructure.ElevenLabs;

internal class ElevenLabsTextToSpeechProvider : ITextToSpeechProvider
{
    private const string BaseUrl = "https://api.elevenlabs.io/v1";
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly string _voiceId;

    public ElevenLabsTextToSpeechProvider(string apiKey, string? voiceId = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ConfigurationException("ElevenLabs API key cannot be empty");
        }

        _apiKey = apiKey;
        _voiceId = voiceId ?? "21m00Tcm4TlvDq8ikWAM";
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("xi-api-key", _apiKey);
    }

    public async Task<byte[]> GenerateSpeechAsync(
        string text,
        string? partOfSpeech = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty", nameof(text));
        }

        try
        {
            var requestBody = new
            {
                text,
                model_id = "eleven_monolingual_v1",
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.75
                }
            };

            string json = JsonSerializer.Serialize(requestBody);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(
                $"{BaseUrl}/text-to-speech/{_voiceId}",
                content,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new ExternalServiceException(
                    "ElevenLabs",
                    $"TTS request failed with status {response.StatusCode}: {errorContent}"
                );
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException(
                "ElevenLabs",
                $"Failed to generate speech: {ex.Message}",
                ex
            );
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException(
                "ElevenLabs",
                $"Unexpected error generating speech: {ex.Message}",
                ex
            );
        }
    }
}
