namespace YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

public class Translation
{
    public int Rank { get; init; }
    public string Word { get; init; } = string.Empty;
    public string PartOfSpeech { get; init; } = string.Empty;
    public string? Countability { get; init; }
    public string? CmuArpabet { get; init; }
    public List<string> Examples { get; init; } = [];
}
