namespace YetAnotherTranslator.Core.Models;

public record GrammarReviewResult(
    string OriginalText,
    bool IsCorrect,
    List<GrammarIssue> GrammarIssues,
    List<VocabularySuggestion> VocabularySuggestions);

public record GrammarIssue(
    string Type,
    string OriginalText,
    string CorrectedText,
    string Explanation);

public record VocabularySuggestion(
    string OriginalWord,
    string SuggestedWord,
    string Reason);
