using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Core.Interfaces;

public interface ITtsProvider
{
    Task<PronunciationResult> GenerateSpeechAsync(
        string text,
        string? partOfSpeech = null,
        CancellationToken cancellationToken = default);
}
