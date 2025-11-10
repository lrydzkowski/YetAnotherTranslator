using Spectre.Console;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;

namespace YetAnotherTranslator.Cli.Display;

public static class GrammarReviewFormatter
{
    public static void Display(GrammarReviewResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        AnsiConsole.MarkupLine("[bold cyan]Grammar and Vocabulary Review[/]");
        AnsiConsole.WriteLine();

        if (result.GrammarIssues.Count == 0 && result.VocabularySuggestions.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]✓ No issues found. The text is grammatically correct![/]");
            return;
        }

        if (result.GrammarIssues.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold red]Grammar Issues:[/]");
            AnsiConsole.WriteLine();

            var grammarTable = new Table();
            grammarTable.Border(TableBorder.Rounded);
            grammarTable.AddColumn("[bold]Issue[/]");
            grammarTable.AddColumn("[bold]Correction[/]");
            grammarTable.AddColumn("[bold]Explanation[/]");

            foreach (var issue in result.GrammarIssues)
            {
                grammarTable.AddRow(
                    Markup.Escape(issue.Issue),
                    Markup.Escape(issue.Correction),
                    Markup.Escape(issue.Explanation)
                );
            }

            AnsiConsole.Write(grammarTable);
            AnsiConsole.WriteLine();
        }

        if (result.VocabularySuggestions.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold yellow]Vocabulary Suggestions:[/]");
            AnsiConsole.WriteLine();

            var vocabTable = new Table();
            vocabTable.Border(TableBorder.Rounded);
            vocabTable.AddColumn("[bold]Original[/]");
            vocabTable.AddColumn("[bold]Suggestion[/]");
            vocabTable.AddColumn("[bold]Context[/]");

            foreach (var suggestion in result.VocabularySuggestions)
            {
                vocabTable.AddRow(
                    Markup.Escape(suggestion.Original),
                    Markup.Escape(suggestion.Suggestion),
                    Markup.Escape(suggestion.Context)
                );
            }

            AnsiConsole.Write(vocabTable);
        }
    }
}
