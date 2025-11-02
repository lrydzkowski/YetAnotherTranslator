using Spectre.Console;
using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Cli.Output;

public static class TranslationFormatter
{
    public static void DisplayTranslation(TranslationResult result)
    {
        var panel = new Panel(BuildTranslationContent(result))
        {
            Header = new PanelHeader($"[bold green]{result.SourceWord}[/] ([italic]{result.SourceLanguage}[/] → [italic]{result.TargetLanguage}[/])"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static Grid BuildTranslationContent(TranslationResult result)
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadRight(2));
        grid.AddColumn(new GridColumn());

        foreach (var translation in result.Translations)
        {
            grid.AddRow(
                $"[bold yellow]{translation.Rank}.[/]",
                $"[bold cyan]{translation.Word}[/]");

            grid.AddRow(
                string.Empty,
                $"[dim]{translation.PartOfSpeech}{GetCountabilityText(translation.Countability)}[/]");

            grid.AddEmptyRow();

            foreach (var example in translation.Examples)
            {
                grid.AddRow(
                    string.Empty,
                    $"[grey]• {example}[/]");
            }

            if (translation != result.Translations.Last())
            {
                grid.AddEmptyRow();
            }
        }

        return grid;
    }

    private static string GetCountabilityText(string? countability)
    {
        if (string.IsNullOrWhiteSpace(countability))
        {
            return string.Empty;
        }

        return $", {countability}";
    }

    public static void DisplayError(string message)
    {
        AnsiConsole.MarkupLine($"[red]Error: {message}[/]");
        AnsiConsole.WriteLine();
    }

    public static void DisplayHelp()
    {
        var table = new Table
        {
            Border = TableBorder.Rounded,
            BorderStyle = new Style(Color.Blue)
        };

        table.AddColumn("[bold]Command[/]");
        table.AddColumn("[bold]Description[/]");

        table.AddRow("[cyan]word[/]", "Translate a single Polish word to English");
        table.AddRow("[cyan]text with spaces[/]", "Translate Polish text to English");
        table.AddRow("[cyan]/history[/] or [cyan]/hist[/]", "View translation history");
        table.AddRow("[cyan]/clear[/] or [cyan]/c[/]", "Clear the screen");
        table.AddRow("[cyan]/help[/] or [cyan]/h[/]", "Show this help message");
        table.AddRow("[cyan]/quit[/] or [cyan]/q[/]", "Exit the application");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void DisplayWelcome()
    {
        var rule = new Rule("[bold green]YetAnotherTranslator[/]")
        {
            Style = Style.Parse("green"),
            Justification = Justify.Left
        };

        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine("[grey]Polish-English Translation Tool[/]");
        AnsiConsole.MarkupLine("[grey]Type [cyan]/help[/] for commands[/]");
        AnsiConsole.WriteLine();
    }
}
