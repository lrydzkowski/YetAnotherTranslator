using FluentAssertions;
using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features;

public class ReplCommandTests : TestBase
{
    [Fact]
    public void CommandParser_QuitCommand_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser();

        // Act
        var quitResult = parser.Parse("/quit");
        var qResult = parser.Parse("/q");

        // Assert
        quitResult.Type.Should().Be(CommandType.Quit);
        qResult.Type.Should().Be(CommandType.Quit);
    }

    [Fact]
    public void CommandParser_ClearCommand_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser();

        // Act
        var clearResult = parser.Parse("/clear");
        var cResult = parser.Parse("/c");

        // Assert
        clearResult.Type.Should().Be(CommandType.Clear);
        cResult.Type.Should().Be(CommandType.Clear);
    }

    [Fact]
    public void CommandParser_HelpCommand_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser();

        // Act
        var helpResult = parser.Parse("/help");

        // Assert
        helpResult.Type.Should().Be(CommandType.Help);
    }

    [Fact]
    public void CommandParser_AllCommands_ParseCorrectly()
    {
        // Arrange
        var parser = new CommandParser();

        // Act & Assert - Test all command variations
        parser.Parse("/t word").Type.Should().Be(CommandType.TranslateWord);
        parser.Parse("/translate word").Type.Should().Be(CommandType.TranslateWord);
        parser.Parse("/tp słowo").Type.Should().Be(CommandType.TranslateWord);
        parser.Parse("/translate-polish słowo").Type.Should().Be(CommandType.TranslateWord);
        parser.Parse("/te word").Type.Should().Be(CommandType.TranslateWord);
        parser.Parse("/translate-english word").Type.Should().Be(CommandType.TranslateWord);

        parser.Parse("/tt text").Type.Should().Be(CommandType.TranslateText);
        parser.Parse("/translate-text text").Type.Should().Be(CommandType.TranslateText);
        parser.Parse("/ttp tekst").Type.Should().Be(CommandType.TranslateText);
        parser.Parse("/translate-text-polish tekst").Type.Should().Be(CommandType.TranslateText);
        parser.Parse("/tte text").Type.Should().Be(CommandType.TranslateText);
        parser.Parse("/translate-text-english text").Type.Should().Be(CommandType.TranslateText);

        parser.Parse("/r text").Type.Should().Be(CommandType.ReviewGrammar);
        parser.Parse("/review text").Type.Should().Be(CommandType.ReviewGrammar);

        parser.Parse("/p word").Type.Should().Be(CommandType.PlayPronunciation);
        parser.Parse("/playback word").Type.Should().Be(CommandType.PlayPronunciation);

        parser.Parse("/hist").Type.Should().Be(CommandType.GetHistory);
        parser.Parse("/history").Type.Should().Be(CommandType.GetHistory);

        parser.Parse("/help").Type.Should().Be(CommandType.Help);
        parser.Parse("/clear").Type.Should().Be(CommandType.Clear);
        parser.Parse("/c").Type.Should().Be(CommandType.Clear);
        parser.Parse("/quit").Type.Should().Be(CommandType.Quit);
        parser.Parse("/q").Type.Should().Be(CommandType.Quit);
    }

    [Fact]
    public void CommandParser_InvalidCommand_ReturnsInvalid()
    {
        // Arrange
        var parser = new CommandParser();

        // Act
        var result = parser.Parse("/unknown");
        var emptyResult = parser.Parse("");
        var noSlashResult = parser.Parse("test");

        // Assert
        result.Type.Should().Be(CommandType.Invalid);
        emptyResult.Type.Should().Be(CommandType.Invalid);
        noSlashResult.Type.Should().Be(CommandType.Invalid);
    }

    [Fact]
    public void CommandParser_NoCacheFlag_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser();

        // Act
        var withCache = parser.Parse("/t word");
        var withoutCache = parser.Parse("/t word --no-cache");
        var withoutCacheReversed = parser.Parse("/t --no-cache word");

        // Assert
        withCache.NoCache.Should().BeFalse();
        withoutCache.NoCache.Should().BeTrue();
        withoutCache.Argument.Should().Be("word");
        withoutCacheReversed.NoCache.Should().BeTrue();
        withoutCacheReversed.Argument.Should().Be("word");
    }
}
