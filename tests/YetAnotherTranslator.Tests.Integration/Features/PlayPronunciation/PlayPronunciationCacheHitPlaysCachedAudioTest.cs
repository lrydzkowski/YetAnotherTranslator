using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.PlayPronunciation;

public class PlayPronunciationCacheHitPlaysCachedAudioTest : TestBase
{
    private PlayPronunciationHandler _handler = null!;
    private TestAudioPlayer _audioPlayer = null!;

    public PlayPronunciationCacheHitPlaysCachedAudioTest(IntegrationTestFixture fixture) : base(fixture)
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
        string word = "test";

        // First call - should generate and cache
        var request1 = new PlayPronunciationRequest(word, null, UseCache: true);
        PronunciationResult result1 = await _handler.HandleAsync(request1);

        // Second call - should use cache
        var request2 = new PlayPronunciationRequest(word, null, UseCache: true);
        PronunciationResult result2 = await _handler.HandleAsync(request2);

        // Assert
        await Verify(new
        {
            FirstResult = result1,
            SecondResult = result2,
            TotalPlayedAudioCount = _audioPlayer.PlayedAudio.Count
        });
    }
}
