namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

public class GrammarReviewResult
{
    public string InputText { get; init; } = string.Empty;
    public List<GrammarIssue> GrammarIssues { get; init; } = new();
    public List<VocabularySuggestion> VocabularySuggestions { get; init; } = new();
}

public class GrammarIssue
{
    public string Issue { get; init; } = string.Empty;
    public string Correction { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
}

public class VocabularySuggestion
{
    public string Original { get; init; } = string.Empty;
    public string Suggestion { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
}
