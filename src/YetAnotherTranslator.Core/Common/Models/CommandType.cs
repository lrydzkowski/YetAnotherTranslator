namespace YetAnotherTranslator.Core.Common.Models;

public enum CommandType
{
    Invalid,
    TranslateWord,
    TranslateText,
    ReviewGrammar,
    PlayPronunciation,
    GetHistory,
    Help,
    Clear,
    Quit
}
