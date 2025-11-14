using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReplCommand;

public class CommandParserInvalidCommandReturnsInvalidTest : TestBase
{
    public CommandParserInvalidCommandReturnsInvalidTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        var parser = new CommandParser();

        var result = parser.Parse("/unknown");
        var emptyResult = parser.Parse("");
        var noSlashResult = parser.Parse("test");

        Verify(new
        {
            UnknownCommandResult = result,
            EmptyCommandResult = emptyResult,
            NoSlashResult = noSlashResult
        });
    }
}
