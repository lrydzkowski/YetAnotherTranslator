using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.Encoding;

public class CommandParserPolishInputPreservesEncodingTest : TestBase
{
    public CommandParserPolishInputPreservesEncodingTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        var parser = new CommandParser();
        string polishInput = "/t książka"; // book

        var command = parser.Parse(polishInput);

        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(command.Argument);
        string decodedText = System.Text.Encoding.UTF8.GetString(utf8Bytes);

        Verify(new
        {
            Command = command,
            Utf8RoundTrip = decodedText,
            EncodingPreserved = decodedText == "książka"
        });
    }
}
