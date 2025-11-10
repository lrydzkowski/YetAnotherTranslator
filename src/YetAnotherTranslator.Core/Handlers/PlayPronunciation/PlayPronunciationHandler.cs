using System.Text.Json;
using FluentValidation;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation;

public class PlayPronunciationHandler
{
    private readonly ITtsProvider _ttsProvider;
    private readonly IAudioPlayer _audioPlayer;
    private readonly IValidator<PlayPronunciationRequest> _validator;
    private readonly IHistoryRepository _historyRepository;
    private const string DefaultVoiceId = "21m00Tcm4TlvDq8ikWAM";

    public PlayPronunciationHandler(
        ITtsProvider ttsProvider,
        IAudioPlayer audioPlayer,
        IValidator<PlayPronunciationRequest> validator,
        IHistoryRepository historyRepository)
    {
        _ttsProvider = ttsProvider ?? throw new ArgumentNullException(nameof(ttsProvider));
        _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
    }

    public async Task<PronunciationResult> HandleAsync(
        PlayPronunciationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        byte[]? audioData = null;

        if (request.UseCache)
        {
            audioData = await _historyRepository.GetCachedPronunciationAsync(
                request.Text,
                request.PartOfSpeech,
                cancellationToken
            );
        }

        if (audioData == null)
        {
            audioData = await _ttsProvider.GenerateSpeechAsync(
                request.Text,
                request.PartOfSpeech,
                cancellationToken
            );

            await _historyRepository.SavePronunciationAsync(
                request.Text,
                request.PartOfSpeech,
                audioData,
                DefaultVoiceId,
                cancellationToken
            );
        }

        await _audioPlayer.PlayAsync(audioData, cancellationToken);

        var result = new PronunciationResult
        {
            Text = request.Text,
            PartOfSpeech = request.PartOfSpeech,
            Played = true
        };

        await _historyRepository.SaveHistoryAsync(
            CommandType.PlayPronunciation,
            request.Text,
            JsonSerializer.Serialize(result),
            cancellationToken
        );

        return result;
    }
}
