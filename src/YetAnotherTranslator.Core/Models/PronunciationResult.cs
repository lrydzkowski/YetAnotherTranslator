namespace YetAnotherTranslator.Core.Models;

public record PronunciationResult(
    string Text,
    string? PartOfSpeech,
    byte[] AudioData,
    string AudioFormat,
    string VoiceId,
    int AudioSizeBytes);
