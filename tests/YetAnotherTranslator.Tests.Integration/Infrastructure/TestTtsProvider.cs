using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Tests.Integration.Infrastructure;

public class TestTtsProvider : ITtsProvider
{
    public Task<byte[]> GenerateSpeechAsync(
        string text,
        string? partOfSpeech = null,
        CancellationToken cancellationToken = default)
    {
        // Return fake audio data (just some bytes to simulate MP3 data)
        var audioData = new byte[] { 0xFF, 0xFB, 0x90, 0x44, 0x00, 0x00, 0x00, 0x00 };
        return Task.FromResult(audioData);
    }
}
