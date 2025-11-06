namespace YetAnotherTranslator.Core.Interfaces;

public interface ITtsProvider
{
    Task<byte[]> GenerateSpeechAsync(
        string text,
        string? partOfSpeech = null,
        CancellationToken cancellationToken = default
    );
}
