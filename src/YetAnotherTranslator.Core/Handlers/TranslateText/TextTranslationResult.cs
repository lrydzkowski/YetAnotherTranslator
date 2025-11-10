namespace YetAnotherTranslator.Core.Handlers.TranslateText;

public class TextTranslationResult
{
    public string SourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
    public string InputText { get; init; } = string.Empty;
    public string TranslatedText { get; init; } = string.Empty;
}
