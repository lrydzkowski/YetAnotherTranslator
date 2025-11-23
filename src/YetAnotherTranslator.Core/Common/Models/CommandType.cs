namespace YetAnotherTranslator.Core.Common.Models;

public enum CommandType
{
    Invalid,
    TranslateWordAutodetect,
    TranslateWordToEnglish,
    TranslateWordToPolish,
    TranslateTextAutodetect,
    TranslateTextToEnglish,
    TranslateTextToPolish,
    ReviewGrammar,
    PlayPronunciation,
    GetHistory,
    Help,
    Clear,
    Quit
}
