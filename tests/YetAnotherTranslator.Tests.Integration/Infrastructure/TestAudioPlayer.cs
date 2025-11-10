using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Tests.Integration.Infrastructure;

public class TestAudioPlayer : IAudioPlayer
{
    public List<byte[]> PlayedAudio { get; } = new();

    public Task PlayAsync(byte[] audioData, CancellationToken cancellationToken = default)
    {
        PlayedAudio.Add(audioData);
        return Task.CompletedTask;
    }
}
