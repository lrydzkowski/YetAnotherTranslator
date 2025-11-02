using YetAnotherTranslator.Core.Interfaces;
using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Infrastructure.Tts;

public class ElevenLabsProvider : ITtsProvider
{
    private readonly string _apiKey;
    private readonly string _voiceId;

    public ElevenLabsProvider(string apiKey, string voiceId)
    {
        _apiKey = apiKey;
        _voiceId = voiceId;
    }

    public Task<PronunciationResult> GenerateSpeechAsync(
        string text,
        string? partOfSpeech = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
