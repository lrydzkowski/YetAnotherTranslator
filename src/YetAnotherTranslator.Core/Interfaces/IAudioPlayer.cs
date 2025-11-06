namespace YetAnotherTranslator.Core.Interfaces;

public interface IAudioPlayer
{
    Task PlayAsync(byte[] audioData, CancellationToken cancellationToken = default);
}
