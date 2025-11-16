namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;

public interface ITextToSpeechProvider
{
    Task<byte[]> GenerateSpeechAsync(
        string text,
        string? partOfSpeech = null,
        CancellationToken cancellationToken = default
    );
}
