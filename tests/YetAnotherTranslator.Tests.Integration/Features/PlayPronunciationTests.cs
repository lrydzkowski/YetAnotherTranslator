using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class PlayPronunciationTests : TestBase
{
    private PlayPronunciationHandler _handler = null!;
    private TestAudioPlayer _audioPlayer = null!;

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
    public async Task PlayPronunciation_Word_PlaysAudio()
    {
        string word = "hello";

        var request = new PlayPronunciationRequest(word, null, UseCache: false);

        PronunciationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.Text.Should().Be(word);
        result.Played.Should().BeTrue();
        _audioPlayer.PlayedAudio.Should().HaveCount(1);
        _audioPlayer.PlayedAudio[0].Should().NotBeEmpty();
    }

    [Fact]
    public async Task PlayPronunciation_Phrase_PlaysAudio()
    {
        string phrase = "hello world";

        var request = new PlayPronunciationRequest(phrase, null, UseCache: false);

        PronunciationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.Text.Should().Be(phrase);
        result.Played.Should().BeTrue();
        _audioPlayer.PlayedAudio.Should().HaveCount(1);
    }

    [Fact]
    public async Task PlayPronunciation_WithPartOfSpeech_PlaysAudio()
    {
        string word = "record";
        string partOfSpeech = "noun";

        var request = new PlayPronunciationRequest(word, partOfSpeech, UseCache: false);

        PronunciationResult result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.Text.Should().Be(word);
        result.PartOfSpeech.Should().Be(partOfSpeech);
        result.Played.Should().BeTrue();
        _audioPlayer.PlayedAudio.Should().HaveCount(1);
    }

    [Fact]
    public async Task PlayPronunciation_CacheHit_PlaysCachedAudio()
    {
        string word = "test";

        // First call - should generate and cache
        var request1 = new PlayPronunciationRequest(word, null, UseCache: true);
        PronunciationResult result1 = await _handler.HandleAsync(request1);

        result1.Should().NotBeNull();
        result1.Text.Should().Be(word);
        result1.Played.Should().BeTrue();

        // Second call - should use cache
        var request2 = new PlayPronunciationRequest(word, null, UseCache: true);
        PronunciationResult result2 = await _handler.HandleAsync(request2);

        result2.Should().NotBeNull();
        result2.Text.Should().Be(word);
        result2.Played.Should().BeTrue();

        // Both should have played audio
        _audioPlayer.PlayedAudio.Should().HaveCount(2);
    }
}
