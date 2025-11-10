using NAudio.Wave;
using PortAudioSharp;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Infrastructure.Tts;

public class PortAudioPlayer : IAudioPlayer, IDisposable
{
    private bool _disposed;
    private static readonly object _portAudioLock = new();
    private static bool _portAudioInitialized;

    public PortAudioPlayer()
    {
        InitializePortAudio();
    }

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
            // ElevenLabs returns MP3 by default, so we need to decode it
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
        using var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);

        // Get audio format information
        var format = waveStream.WaveFormat;
        int sampleRate = format.SampleRate;
        int channels = format.Channels;

        // Read all PCM data
        var pcmData = new List<byte>();
        var buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = waveStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            for (int i = 0; i < bytesRead; i++)
            {
                pcmData.Add(buffer[i]);
            }
        }

        if (pcmData.Count == 0)
        {
            throw new InvalidOperationException("No audio data to play");
        }

        // Play the PCM data using PortAudio
        PlayPcmData(pcmData.ToArray(), sampleRate, channels, cancellationToken);
    }

    private void PlayPcmData(byte[] pcmData, int sampleRate, int channels, CancellationToken cancellationToken)
    {
        var outputParams = new PortAudio.StreamParameters
        {
            device = PortAudio.DefaultOutputDevice,
            channelCount = channels,
            sampleFormat = PortAudio.SampleFormat.Int16,
            suggestedLatency = PortAudio.HighOutputLatency
        };

        IntPtr stream = IntPtr.Zero;

        try
        {
            // Open stream
            PortAudio.OpenStream(
                out stream,
                IntPtr.Zero, // no input
                ref outputParams,
                sampleRate,
                1024, // frames per buffer
                PortAudio.StreamFlags.NoFlag,
                null, // no callback, we'll use blocking write
                IntPtr.Zero
            );

            // Start stream
            PortAudio.StartStream(stream);

            // Convert byte array to short array (16-bit PCM)
            int sampleCount = pcmData.Length / 2;
            short[] samples = new short[sampleCount];
            Buffer.BlockCopy(pcmData, 0, samples, 0, pcmData.Length);

            // Write audio data to stream
            int framesPerBuffer = 1024;
            int offset = 0;

            while (offset < sampleCount)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                int framesToWrite = Math.Min(framesPerBuffer * channels, sampleCount - offset);
                short[] chunk = new short[framesToWrite];
                Array.Copy(samples, offset, chunk, 0, framesToWrite);

                PortAudio.WriteStream(stream, chunk, framesToWrite / channels);

                offset += framesToWrite;
            }

            // Wait for playback to complete
            while (PortAudio.IsStreamActive(stream) != 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(10);
            }

            // Stop stream
            PortAudio.StopStream(stream);
        }
        finally
        {
            // Close stream
            if (stream != IntPtr.Zero)
            {
                PortAudio.CloseStream(stream);
            }
        }
    }

    private static void InitializePortAudio()
    {
        lock (_portAudioLock)
        {
            if (_portAudioInitialized)
            {
                return;
            }

            try
            {
                var error = PortAudio.Initialize();
                if (error != PortAudio.PaError.NoError)
                {
                    throw new InvalidOperationException(
                        $"Failed to initialize PortAudio: {PortAudio.GetErrorText(error)}"
                    );
                }

                _portAudioInitialized = true;

                // Register termination handler
                AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
                {
                    TerminatePortAudio();
                };
            }
            catch (DllNotFoundException ex)
            {
                throw new InvalidOperationException(
                    "PortAudio library not found. Please install PortAudio:\n" +
                    "- Windows: Included with PortAudioSharp NuGet package\n" +
                    "- macOS: brew install portaudio\n" +
                    "- Linux: sudo apt-get install portaudio19-dev (Debian/Ubuntu) or sudo dnf install portaudio-devel (Fedora)",
                    ex
                );
            }
        }
    }

    private static void TerminatePortAudio()
    {
        lock (_portAudioLock)
        {
            if (_portAudioInitialized)
            {
                try
                {
                    PortAudio.Terminate();
                }
                catch
                {
                    // Ignore errors during termination
                }
                finally
                {
                    _portAudioInitialized = false;
                }
            }
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
