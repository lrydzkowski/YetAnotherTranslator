using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReplCommand;

public class CommandParserClearCommandParsesCorrectlyTest : TestBase
{
    public CommandParserClearCommandParsesCorrectlyTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        // Arrange
        var parser = new CommandParser();

        // Act
        var clearResult = parser.Parse("/clear");
        var cResult = parser.Parse("/c");

        // Assert
        Verify(new
        {
            ClearResult = clearResult,
            CResult = cResult
        });
    }
}
