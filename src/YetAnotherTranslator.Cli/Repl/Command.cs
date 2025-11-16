using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Cli.Repl;

public class Command
{
    public CommandType Type { get; init; }
    public string Argument { get; init; } = string.Empty;
    public string? SourceLanguage { get; init; }
    public string? TargetLanguage { get; init; }
    public bool AutoDetectLanguage { get; init; }
    public bool NoCache { get; init; }
}
