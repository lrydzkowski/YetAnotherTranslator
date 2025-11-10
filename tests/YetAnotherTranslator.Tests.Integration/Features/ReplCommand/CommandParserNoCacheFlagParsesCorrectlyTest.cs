using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReplCommand;

public class CommandParserNoCacheFlagParsesCorrectlyTest : TestBase
{
    public CommandParserNoCacheFlagParsesCorrectlyTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        // Arrange
        var parser = new CommandParser();

        // Act
        var withCache = parser.Parse("/t word");
        var withoutCache = parser.Parse("/t word --no-cache");
        var withoutCacheReversed = parser.Parse("/t --no-cache word");

        // Assert
        Verify(new
        {
            WithCache = withCache,
            WithoutCache = withoutCache,
            WithoutCacheReversed = withoutCacheReversed
        });
    }
}
