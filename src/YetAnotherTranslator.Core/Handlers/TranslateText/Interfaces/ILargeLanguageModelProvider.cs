namespace YetAnotherTranslator.Core.Handlers.TranslateText.Interfaces;

public interface ILargeLanguageModelProvider
{
    Task<string> TranslateTextAsync(
        string text,
        string? sourceLanguage,
        string? targetLanguage,
        CancellationToken cancellationToken = default
    );
}
