using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Core.Interfaces;

public interface ILlmProvider
{
    Task<TranslationResult> TranslateWordAsync(
        string word,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default);

    Task<TextTranslationResult> TranslateTextAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default);

    Task<GrammarReviewResult> ReviewGrammarAsync(
        string text,
        CancellationToken cancellationToken = default);

    Task<string> DetectLanguageAsync(
        string text,
        CancellationToken cancellationToken = default);
}
