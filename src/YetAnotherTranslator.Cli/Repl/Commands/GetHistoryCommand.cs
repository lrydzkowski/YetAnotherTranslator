using Spectre.Console;
using YetAnotherTranslator.Core.Common.Models;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.GetHistory.Models;

namespace YetAnotherTranslator.Cli.Repl.Commands;

internal class GetHistoryCommand
{
    private readonly IGetHistoryHandler _getHistoryHandler;

    public GetHistoryCommand(IGetHistoryHandler getHistoryHandler)
    {
        _getHistoryHandler = getHistoryHandler;
    }

    public async Task HandleGetHistoryAsync(CancellationToken cancellationToken)
    {
        await AnsiConsole.Status()
            .StartAsync(
                "Retrieving history...",
                async ctx =>
                {
                    GetHistoryResult result = await _getHistoryHandler.HandleAsync(
                        new GetHistoryRequest(),
                        cancellationToken
                    );
                    ctx.Status("Done");
                    Display(result);
                }
            );
    }

    private void Display(GetHistoryResult result)
    {
        AnsiConsole.WriteLine();
        if (result.Entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No history entries found.[/]");
            return;
        }

        Table table = new();
        table.Border(TableBorder.Rounded);
        table.AddColumn(new TableColumn("[bold]Timestamp[/]"));
        table.AddColumn(new TableColumn("[bold]Command[/]"));
        table.AddColumn(new TableColumn("[bold]Input[/]"));

        foreach (HistoryEntry entry in result.Entries)
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
            CommandType.TranslateWordAutodetect => "[cyan]Translate Word[/]",
            CommandType.TranslateWordToEnglish => "[cyan]Translate Word to English[/]",
            CommandType.TranslateWordToPolish => "[cyan]Translate Word to Polish[/]",
            CommandType.TranslateTextAutodetect => "[cyan]Translate Text[/]",
            CommandType.TranslateTextToEnglish => "[cyan]Translate Text to English[/]",
            CommandType.TranslateTextToPolish => "[cyan]Translate Text to Polish[/]",
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
