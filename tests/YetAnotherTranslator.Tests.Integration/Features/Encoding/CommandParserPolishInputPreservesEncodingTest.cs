using System.Text;
using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Tests.Integration.Infrastructure;
using Encoding = System.Text.Encoding;

namespace YetAnotherTranslator.Tests.Integration.Features.Encoding;

public class CommandParserPolishInputPreservesEncodingTest : TestBase
{
    public CommandParserPolishInputPreservesEncodingTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        // Arrange
        var parser = new CommandParser();
        string polishInput = "/t książka"; // book

        // Act
        var command = parser.Parse(polishInput);

        // Verify UTF-8 encoding is preserved
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(command.Argument);
        string decodedText = Encoding.UTF8.GetString(utf8Bytes);

        // Assert
        Verify(new
        {
            Command = command,
            Utf8RoundTrip = decodedText,
            EncodingPreserved = decodedText == "książka"
        });
    }
}
