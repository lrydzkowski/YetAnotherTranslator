using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar.Interfaces;

public interface ILargeLanguageModelProvider
{
    Task<GrammarReviewResult?> ReviewGrammarAsync(
        string text,
        CancellationToken cancellationToken = default
    );
}
