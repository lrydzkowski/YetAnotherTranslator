namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

public class GrammarReviewResult
{
    public string InputText { get; set; } = string.Empty;
    public List<GrammarIssue> GrammarIssues { get; init; } = [];
    public List<VocabularySuggestion> VocabularySuggestions { get; init; } = [];
    public string ModifiedText { get; init; } = string.Empty;
}
