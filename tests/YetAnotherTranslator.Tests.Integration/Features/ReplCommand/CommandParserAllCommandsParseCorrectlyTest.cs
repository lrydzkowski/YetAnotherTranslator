using YetAnotherTranslator.Cli.Repl;
using YetAnotherTranslator.Tests.Integration.Infrastructure;

namespace YetAnotherTranslator.Tests.Integration.Features.ReplCommand;

public class CommandParserAllCommandsParseCorrectlyTest : TestBase
{
    public CommandParserAllCommandsParseCorrectlyTest(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void Run()
    {
        var parser = new CommandParser();

        var results = new
        {
            TranslateWordT = parser.Parse("/t word"),
            TranslateWordTranslate = parser.Parse("/translate word"),
            TranslateWordTp = parser.Parse("/tp słowo"),
            TranslateWordTranslatePolish = parser.Parse("/translate-polish słowo"),
            TranslateWordTe = parser.Parse("/te word"),
            TranslateWordTranslateEnglish = parser.Parse("/translate-english word"),

            TranslateTextTt = parser.Parse("/tt text"),
            TranslateTextTranslateText = parser.Parse("/translate-text text"),
            TranslateTextTtp = parser.Parse("/ttp tekst"),
            TranslateTextTranslateTextPolish = parser.Parse("/translate-text-polish tekst"),
            TranslateTextTte = parser.Parse("/tte text"),
            TranslateTextTranslateTextEnglish = parser.Parse("/translate-text-english text"),

            ReviewGrammarR = parser.Parse("/r text"),
            ReviewGrammarReview = parser.Parse("/review text"),

            PlayPronunciationP = parser.Parse("/p word"),
            PlayPronunciationPlayback = parser.Parse("/playback word"),

            GetHistoryHist = parser.Parse("/hist"),
            GetHistoryHistory = parser.Parse("/history"),

            Help = parser.Parse("/help"),
            Clear = parser.Parse("/clear"),
            ClearC = parser.Parse("/c"),
            Quit = parser.Parse("/quit"),
            QuitQ = parser.Parse("/q")
        };

        Verify(results);
    }
}
