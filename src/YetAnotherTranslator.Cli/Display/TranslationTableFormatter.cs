using Spectre.Console;
using YetAnotherTranslator.Core.Handlers.TranslateWord;

namespace YetAnotherTranslator.Cli.Display;

public static class TranslationTableFormatter
{
    public static void Display(TranslationResult result)
    {
        if (result == null || result.Translations.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No translations found.[/]");
            return;
        }

        bool showArpabet = result.SourceLanguage == "Polish" && result.TargetLanguage == "English";

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(new TableColumn("[bold]Rank[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Word[/]"));
        table.AddColumn(new TableColumn("[bold]Part of Speech[/]"));
        table.AddColumn(new TableColumn("[bold]Countability[/]"));

        if (showArpabet)
        {
            table.AddColumn(new TableColumn("[bold]CMU Arpabet[/]"));
        }

        table.AddColumn(new TableColumn("[bold]Examples[/]"));

        foreach (Translation translation in result.Translations)
        {
            var row = new List<string>
            {
                translation.Rank.ToString(),
                Markup.Escape(translation.Word),
                Markup.Escape(translation.PartOfSpeech),
                Markup.Escape(translation.Countability ?? "N/A")
            };

            if (showArpabet)
            {
                row.Add(Markup.Escape(translation.CmuArpabet ?? "N/A"));
            }

            string examples = string.Join("\n", translation.Examples.Select(Markup.Escape));
            row.Add(examples);

            table.AddRow(row.ToArray());
        }

        AnsiConsole.Write(table);
    }
}
