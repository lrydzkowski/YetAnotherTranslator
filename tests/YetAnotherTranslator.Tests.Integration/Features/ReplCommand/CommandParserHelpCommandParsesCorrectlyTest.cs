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
        // Arrange
        var parser = new CommandParser();

        // Act
        var helpResult = parser.Parse("/help");

        // Assert
        Verify(helpResult);
    }
}
