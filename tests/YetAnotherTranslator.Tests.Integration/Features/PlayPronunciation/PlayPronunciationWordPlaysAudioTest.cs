using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.PlayPronunciation;

public class PlayPronunciationWordPlaysAudioTest : TestBase
{
    private PlayPronunciationHandler _handler = null!;
    private TestAudioPlayer _audioPlayer = null!;

    public PlayPronunciationWordPlaysAudioTest(IntegrationTestFixture fixture) : base(fixture)
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
        // Arrange
        string word = "hello";
        var request = new PlayPronunciationRequest(word, null, UseCache: false);

        // Act
        PronunciationResult result = await _handler.HandleAsync(request);

        // Assert
        await Verify(new
        {
            Result = result,
            PlayedAudioCount = _audioPlayer.PlayedAudio.Count,
            HasAudioData = _audioPlayer.PlayedAudio.Count > 0 && _audioPlayer.PlayedAudio[0].Length > 0
        });
    }
}
