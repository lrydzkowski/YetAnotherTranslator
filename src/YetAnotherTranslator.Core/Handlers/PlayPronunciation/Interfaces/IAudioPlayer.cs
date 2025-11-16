namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;

public interface IAudioPlayer
{
    Task PlayAsync(byte[] audioData, CancellationToken cancellationToken = default);
}
