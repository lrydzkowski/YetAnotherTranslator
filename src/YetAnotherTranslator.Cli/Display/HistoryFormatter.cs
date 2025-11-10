using Spectre.Console;
using YetAnotherTranslator.Core.Handlers.GetHistory;

namespace YetAnotherTranslator.Cli.Display;

public static class HistoryFormatter
{
    public static void Display(GetHistoryResult result)
    {
        if (result.Entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No history entries found.[/]");
            return;
        }

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(new TableColumn("[bold]Timestamp[/]"));
        table.AddColumn(new TableColumn("[bold]Command[/]"));
        table.AddColumn(new TableColumn("[bold]Input[/]"));

        foreach (var entry in result.Entries)
        {
            string timestamp = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            string command = FormatCommandType(entry.CommandType);
            string input = TruncateText(entry.InputText, 50);

            table.AddRow(
                timestamp,
                command,
                Markup.Escape(input)
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]Showing {result.Entries.Count} most recent operations[/]");
    }

    private static string FormatCommandType(CommandType commandType)
    {
        return commandType switch
        {
            CommandType.TranslateWord => "[cyan]Translate Word[/]",
            CommandType.TranslateText => "[cyan]Translate Text[/]",
            CommandType.ReviewGrammar => "[green]Review Grammar[/]",
            CommandType.PlayPronunciation => "[magenta]Play Pronunciation[/]",
            CommandType.GetHistory => "[blue]Get History[/]",
            _ => commandType.ToString()
        };
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.Length <= maxLength)
        {
            return text;
        }

        return text.Substring(0, maxLength - 3) + "...";
    }
}
