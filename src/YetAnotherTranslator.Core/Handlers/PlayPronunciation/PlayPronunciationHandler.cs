using FluentValidation;
using YetAnotherTranslator.Core.Common.Models;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Interfaces;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Models;

namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation;

public interface IPlayPronunciationHandler
{
    Task<PronunciationResult> HandleAsync(
        PlayPronunciationRequest request,
        CancellationToken cancellationToken = default
    );
}

internal class PlayPronunciationHandler : IPlayPronunciationHandler
{
    private const string DefaultVoiceId = "21m00Tcm4TlvDq8ikWAM";
    private readonly IAudioPlayer _audioPlayer;
    private readonly ICacheRepository _cacheRepository;
    private readonly IHistoryRepository _historyRepository;
    private readonly ISerializer _serializer;
    private readonly ITextToSpeechProvider _textToSpeechProvider;
    private readonly IValidator<PlayPronunciationRequest> _validator;

    public PlayPronunciationHandler(
        ITextToSpeechProvider textToSpeechProvider,
        IAudioPlayer audioPlayer,
        IValidator<PlayPronunciationRequest> validator,
        IHistoryRepository historyRepository,
        ICacheRepository cacheRepository,
        ISerializer serializer
    )
    {
        _textToSpeechProvider = textToSpeechProvider;
        _audioPlayer = audioPlayer;
        _validator = validator;
        _historyRepository = historyRepository;
        _cacheRepository = cacheRepository;
        _serializer = serializer;
    }

    public async Task<PronunciationResult> HandleAsync(
        PlayPronunciationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        byte[]? audioData = null;
        if (request.UseCache)
        {
            audioData = await _cacheRepository.GetPronunciationAsync(
                request.Text,
                request.PartOfSpeech,
                cancellationToken
            );
        }

        if (audioData is null)
        {
            audioData = await _textToSpeechProvider.GenerateSpeechAsync(
                request.Text,
                request.PartOfSpeech,
                cancellationToken
            );

            await _cacheRepository.SavePronunciationAsync(
                request.Text,
                request.PartOfSpeech,
                audioData,
                DefaultVoiceId,
                cancellationToken
            );
        }

        await _audioPlayer.PlayAsync(audioData, cancellationToken);

        PronunciationResult result = new()
        {
            Text = request.Text,
            PartOfSpeech = request.PartOfSpeech,
            Played = true
        };

        await _historyRepository.SaveHistoryAsync(
            CommandType.PlayPronunciation,
            request.Text,
            _serializer.Serialize(result),
            cancellationToken
        );

        return result;
    }
}
