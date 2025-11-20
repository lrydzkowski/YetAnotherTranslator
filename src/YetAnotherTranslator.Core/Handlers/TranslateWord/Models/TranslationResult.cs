namespace YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

public class TranslationResult
{
    public string? SourceLanguage { get; set; }
    public string? TargetLanguage { get; set; }
    public string InputText { get; set; } = string.Empty;
    public List<Translation> Translations { get; init; } = [];
}
