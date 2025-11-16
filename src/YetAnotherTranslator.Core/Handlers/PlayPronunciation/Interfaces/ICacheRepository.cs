namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;

public interface ICacheRepository
{
    Task<byte[]?> GetPronunciationAsync(
        string text,
        string? partOfSpeech,
        CancellationToken cancellationToken = default
    );

    Task SavePronunciationAsync(
        string text,
        string? partOfSpeech,
        byte[] audioData,
        string voiceId,
        CancellationToken cancellationToken = default
    );
}
