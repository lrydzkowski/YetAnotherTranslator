namespace YetAnotherTranslator.Core.Commands;

public record TranslateWordCommand(
    string Word,
    string SourceLanguage,
    string TargetLanguage);
