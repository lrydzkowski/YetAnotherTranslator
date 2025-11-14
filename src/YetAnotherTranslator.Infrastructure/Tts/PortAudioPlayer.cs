using NAudio.Wave;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Infrastructure.Tts;

public class PortAudioPlayer : IAudioPlayer, IDisposable
{
    private bool _disposed;

    public async Task PlayAsync(byte[] audioData, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(PortAudioPlayer));
        }

        if (audioData == null || audioData.Length == 0)
        {
            throw new ArgumentException("Audio data cannot be empty", nameof(audioData));
        }

        try
        {
            await Task.Run(() => PlayMp3Audio(audioData, cancellationToken), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to play audio: {ex.Message}. " +
                "Ensure audio drivers are installed and accessible.",
                ex
            );
        }
    }

    private void PlayMp3Audio(byte[] audioData, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream(audioData);
        using var mp3Reader = new Mp3FileReader(memoryStream);
        using var waveOut = new WaveOutEvent();

        waveOut.Init(mp3Reader);
        waveOut.Play();

        while (waveOut.PlaybackState == PlaybackState.Playing)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                waveOut.Stop();
                return;
            }

            Thread.Sleep(100);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~PortAudioPlayer()
    {
        Dispose();
    }
}
