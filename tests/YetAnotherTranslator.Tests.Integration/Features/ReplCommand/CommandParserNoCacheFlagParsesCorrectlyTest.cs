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
        var parser = new CommandParser();

        var withCache = parser.Parse("/t word");
        var withoutCache = parser.Parse("/t word --no-cache");
        var withoutCacheReversed = parser.Parse("/t --no-cache word");

        Verify(new
        {
            WithCache = withCache,
            WithoutCache = withoutCache,
            WithoutCacheReversed = withoutCacheReversed
        });
    }
}
