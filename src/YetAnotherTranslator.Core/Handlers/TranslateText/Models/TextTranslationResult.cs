namespace YetAnotherTranslator.Core.Handlers.TranslateText.Models;

public class TextTranslationResult
{
    public string? SourceLanguage { get; init; }
    public string? TargetLanguage { get; init; }
    public string InputText { get; init; } = string.Empty;
    public string TranslatedText { get; init; } = string.Empty;
}
