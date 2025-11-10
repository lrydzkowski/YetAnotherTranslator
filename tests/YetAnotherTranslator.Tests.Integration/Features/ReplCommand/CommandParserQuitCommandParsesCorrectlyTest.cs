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
        // Arrange
        var parser = new CommandParser();

        // Act
        var quitResult = parser.Parse("/quit");
        var qResult = parser.Parse("/q");

        // Assert
        Verify(new
        {
            QuitResult = quitResult,
            QResult = qResult
        });
    }
}
