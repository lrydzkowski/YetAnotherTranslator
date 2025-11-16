namespace YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

public class TranslationResult
{
    public string? SourceLanguage { get; init; }
    public string? TargetLanguage { get; init; }
    public string InputText { get; init; } = string.Empty;
    public List<Translation> Translations { get; init; } = [];
}
