namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public class TranslationResult
{
    public string SourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
    public string InputText { get; init; } = string.Empty;
    public List<Translation> Translations { get; init; } = new();
}
