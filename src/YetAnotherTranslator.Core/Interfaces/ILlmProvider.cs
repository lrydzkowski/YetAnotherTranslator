namespace YetAnotherTranslator.Core.Interfaces;

public interface ILlmProvider
{
    Task<string> DetectLanguageAsync(string text, CancellationToken cancellationToken = default);

    Task<string> TranslateWordAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default
    );

    Task<string> TranslateTextAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default
    );

    Task<string> ReviewGrammarAsync(
        string text,
        CancellationToken cancellationToken = default
    );
}
