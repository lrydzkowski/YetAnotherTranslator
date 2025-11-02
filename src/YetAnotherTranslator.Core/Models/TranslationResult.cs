namespace YetAnotherTranslator.Core.Models;

public record TranslationResult(
    string SourceWord,
    string SourceLanguage,
    string TargetLanguage,
    List<Translation> Translations,
    string? DetectedLanguage = null);

public record Translation(
    int Rank,
    string Word,
    string PartOfSpeech,
    string? Countability,
    List<string> Examples);
