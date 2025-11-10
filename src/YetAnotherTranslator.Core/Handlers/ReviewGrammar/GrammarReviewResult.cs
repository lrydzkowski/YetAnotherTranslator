namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

public class GrammarReviewResult
{
    public string InputText { get; init; } = string.Empty;
    public List<GrammarIssue> GrammarIssues { get; init; } = new();
    public List<VocabularySuggestion> VocabularySuggestions { get; init; } = new();
}
