using Spectre.Console;
using YetAnotherTranslator.Core;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

namespace YetAnotherTranslator.Cli.Repl.Commands;

internal class TranslateWordCommand
{
    private readonly ITranslateWordHandler _translateWordHandler;

    public TranslateWordCommand(ITranslateWordHandler translateWordHandler)
    {
        _translateWordHandler = translateWordHandler;
    }

    public async Task HandleTranslateWordAsync(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            AnsiConsole.MarkupLine("[yellow]Please provide a word to translate.[/]");
            return;
        }

        await AnsiConsole.Status()
            .StartAsync(
                "Translating...",
                async ctx =>
                {
                    TranslateWordRequest request = new(
                        command.Type,
                        command.Argument,
                        command.SourceLanguage,
                        command.TargetLanguage,
                        !command.NoCache
                    );
                    TranslationResult result = await _translateWordHandler.HandleAsync(request, cancellationToken);
                    ctx.Status("Done");
                    Display(result);
                }
            );
    }

    private static void Display(TranslationResult result)
    {
        AnsiConsole.WriteLine();

        if (result.Translations.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No translations found.[/]");
            return;
        }

        bool showArpabet = result is
        {
            SourceLanguage: TranslatorConstants.Languages.Polish,
            TargetLanguage: TranslatorConstants.Languages.English
        };

        Table table = new();
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
            List<string> row =
            [
                translation.Rank.ToString(),
                Markup.Escape(translation.Word),
                Markup.Escape(translation.PartOfSpeech),
                Markup.Escape(translation.Countability ?? "N/A")
            ];

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
