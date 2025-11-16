namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

public class GrammarReviewResult
{
    public string InputText { get; init; } = string.Empty;
    public List<GrammarIssue> GrammarIssues { get; init; } = new();
    public List<VocabularySuggestion> VocabularySuggestions { get; init; } = new();
}
