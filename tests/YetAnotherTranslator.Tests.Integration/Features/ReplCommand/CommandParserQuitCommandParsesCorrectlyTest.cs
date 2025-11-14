using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReplCommand;

public class CommandParserQuitCommandParsesCorrectlyTest : TestBase
{
    public CommandParserQuitCommandParsesCorrectlyTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        var parser = new CommandParser();

        var quitResult = parser.Parse("/quit");
        var qResult = parser.Parse("/q");

        Verify(new
        {
            QuitResult = quitResult,
            QResult = qResult
        });
    }
}
