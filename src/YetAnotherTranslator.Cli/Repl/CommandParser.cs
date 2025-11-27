using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Cli.Repl;

internal class CommandParser
{
    public Command Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.StartsWith('/'))
        {
            return new Command { Type = CommandType.Invalid };
        }

        bool noCache = input.Contains("--no-cache");
        if (noCache)
        {
            input = input.Replace("--no-cache", "").Trim();
        }

        string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string commandName = parts[0].ToLowerInvariant();
        string argument = parts.Length > 1 ? parts[1] : string.Empty;

        return commandName switch
        {
            "/t" or "/translate" => new Command
            {
                Type = CommandType.TranslateWordAutodetect,
                Argument = argument,
                AutoDetectLanguage = true,
                NoCache = noCache
            },
            "/tp" or "/translate-polish" => new Command
            {
                Type = CommandType.TranslateWordToEnglish,
                Argument = argument,
                SourceLanguage = "Polish",
                TargetLanguage = "English",
                NoCache = noCache
            },
            "/te" or "/translate-english" => new Command
            {
                Type = CommandType.TranslateWordToPolish,
                Argument = argument,
                SourceLanguage = "English",
                TargetLanguage = "Polish",
                NoCache = noCache
            },
            "/tt" or "/translate-text" => new Command
            {
                Type = CommandType.TranslateTextAutodetect,
                Argument = argument.Replace("\\n", "\n"),
                AutoDetectLanguage = true,
                NoCache = noCache
            },
            "/ttp" or "/translate-text-polish" => new Command
            {
                Type = CommandType.TranslateTextToEnglish,
                Argument = argument.Replace("\\n", "\n"),
                SourceLanguage = "Polish",
                TargetLanguage = "English",
                NoCache = noCache
            },
            "/tte" or "/translate-text-english" => new Command
            {
                Type = CommandType.TranslateTextToPolish,
                Argument = argument.Replace("\\n", "\n"),
                SourceLanguage = "English",
                TargetLanguage = "Polish",
                NoCache = noCache
            },
            "/r" or "/review" => new Command
            {
                Type = CommandType.ReviewGrammar,
                Argument = argument
            },
            "/hist" or "/history" => new Command
            {
                Type = CommandType.GetHistory
            },
            "/h" or "/help" => new Command
            {
                Type = CommandType.Help
            },
            "/c" or "/clear" => new Command
            {
                Type = CommandType.Clear
            },
            "/q" or "/quit" => new Command
            {
                Type = CommandType.Quit
            },
            _ => new Command { Type = CommandType.Invalid }
        };
    }
}
