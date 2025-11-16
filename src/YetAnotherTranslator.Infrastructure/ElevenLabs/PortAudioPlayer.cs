using NAudio.Wave;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;

namespace YetAnotherTranslator.Infrastructure.ElevenLabs;

internal class PortAudioPlayer : IAudioPlayer
{
    public async Task PlayAsync(byte[] audioData, CancellationToken cancellationToken = default)
    {
        if (audioData is null || audioData.Length == 0)
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
                $"Failed to play audio: {ex.Message}. " + "Ensure audio drivers are installed and accessible.",
                ex
            );
        }
    }

    private void PlayMp3Audio(byte[] audioData, CancellationToken cancellationToken)
    {
        using MemoryStream memoryStream = new(audioData);
        using Mp3FileReader mp3Reader = new(memoryStream);
        using WaveOutEvent waveOut = new();

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
}
