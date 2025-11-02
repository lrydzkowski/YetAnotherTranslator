namespace YetAnotherTranslator.Core.Models;

public record TextTranslationResult(
    string SourceText,
    string TranslatedText,
    string SourceLanguage,
    string TargetLanguage,
    int CharacterCount,
    string? DetectedLanguage = null);
