using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;

namespace YetAnotherTranslator.Infrastructure.ElevenLabs;

internal class ElevenLabsApiClient : ITextToSpeechProvider
{
    public Task<byte[]> GenerateSpeechAsync(
        string text,
        string? partOfSpeech = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
