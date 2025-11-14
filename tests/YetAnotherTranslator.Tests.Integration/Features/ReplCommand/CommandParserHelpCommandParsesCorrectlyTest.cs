using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReplCommand;

public class CommandParserHelpCommandParsesCorrectlyTest : TestBase
{
    public CommandParserHelpCommandParsesCorrectlyTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        var parser = new CommandParser();

        var helpResult = parser.Parse("/help");

        Verify(helpResult);
    }
}
