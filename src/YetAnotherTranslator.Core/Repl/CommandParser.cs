using YetAnotherTranslator.Core.Commands;
using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Core.Repl;

public static class CommandParser
{
    public static ParsedCommand Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new ParsedCommand(CommandType.Invalid, null);
        }

        var trimmed = input.Trim();

        if (trimmed.Equals("/quit", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/q", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandType.Quit, null);
        }

        if (trimmed.Equals("/help", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/h", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandType.Help, null);
        }

        if (trimmed.Equals("/clear", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/c", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandType.Clear, null);
        }

        if (trimmed.Equals("/history", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/hist", StringComparison.OrdinalIgnoreCase))
        {
            return new ParsedCommand(CommandType.ViewHistory, null);
        }

        if (!trimmed.Contains(' '))
        {
            return new ParsedCommand(
                CommandType.TranslateWord,
                new TranslateWordCommand(trimmed, "Polish", "English"));
        }

        return new ParsedCommand(CommandType.TranslateText, null);
    }
}

public record ParsedCommand(CommandType Type, object? Data);
