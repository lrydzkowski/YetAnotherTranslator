using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.PlayPronunciation;

public class PlayPronunciationWithPartOfSpeechPlaysAudioTest : TestBase
{
    private PlayPronunciationHandler _handler = null!;
    private TestAudioPlayer _audioPlayer = null!;

    public PlayPronunciationWithPartOfSpeechPlaysAudioTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var validator = new PlayPronunciationValidator();
        var ttsProvider = new TestTtsProvider();
        _audioPlayer = new TestAudioPlayer();
        var historyRepository = ServiceProvider.GetRequiredService<Core.Interfaces.IHistoryRepository>();

        _handler = new PlayPronunciationHandler(
            ttsProvider,
            _audioPlayer,
            validator,
            historyRepository
        );
    }

    [Fact]
    public async Task Run()
    {
        string word = "record";
        string partOfSpeech = "noun";
        var request = new PlayPronunciationRequest(word, partOfSpeech, UseCache: false);

        PronunciationResult result = await _handler.HandleAsync(request);

        await Verify(new
        {
            Result = result,
            PlayedAudioCount = _audioPlayer.PlayedAudio.Count
        });
    }
}
