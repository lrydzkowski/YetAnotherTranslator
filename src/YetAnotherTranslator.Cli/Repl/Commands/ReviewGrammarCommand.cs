using Spectre.Console;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

namespace YetAnotherTranslator.Cli.Repl.Commands;

internal class ReviewGrammarCommand
{
    private readonly ReviewGrammarHandler _reviewGrammarHandler;

    public ReviewGrammarCommand(ReviewGrammarHandler reviewGrammarHandler)
    {
        _reviewGrammarHandler = reviewGrammarHandler;
    }

    public async Task HandleReviewGrammarAsync(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            AnsiConsole.MarkupLine("[yellow]Please provide text to review.[/]");
            return;
        }

        await AnsiConsole.Status()
            .StartAsync(
                "Reviewing grammar...",
                async ctx =>
                {
                    ReviewGrammarRequest request = new(command.Argument, !command.NoCache);
                    GrammarReviewResult? result = await _reviewGrammarHandler.HandleAsync(request, cancellationToken);
                    ctx.Status("Done");
                    Display(result);
                }
            );
    }

    private static void Display(GrammarReviewResult? result)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Grammar and Vocabulary Review[/]");
        AnsiConsole.WriteLine();

        if (result is null)
        {
            AnsiConsole.MarkupLine("[red]No result![/]");

            return;
        }

        if (result.GrammarIssues.Count == 0 && result.VocabularySuggestions.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]✓ No issues found. The text is grammatically correct![/]");

            return;
        }

        AnsiConsole.MarkupLine("[bold]Original Text:[/]");
        AnsiConsole.MarkupLine($"[dim]{Markup.Escape(result.InputText)}[/]");
        AnsiConsole.WriteLine();

        if (result.GrammarIssues.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold red]Grammar Issues:[/]");
            AnsiConsole.WriteLine();

            Table grammarTable = new();
            grammarTable.Border(TableBorder.Rounded);
            grammarTable.AddColumn("[bold]Issue[/]");
            grammarTable.AddColumn("[bold]Correction[/]");
            grammarTable.AddColumn("[bold]Explanation[/]");

            foreach (GrammarIssue issue in result.GrammarIssues)
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

            Table vocabTable = new();
            vocabTable.Border(TableBorder.Rounded);
            vocabTable.AddColumn("[bold]Original[/]");
            vocabTable.AddColumn("[bold]Suggestion[/]");
            vocabTable.AddColumn("[bold]Context[/]");

            foreach (VocabularySuggestion suggestion in result.VocabularySuggestions)
            {
                vocabTable.AddRow(
                    Markup.Escape(suggestion.Original),
                    Markup.Escape(suggestion.Suggestion),
                    Markup.Escape(suggestion.Context)
                );
            }

            AnsiConsole.Write(vocabTable);
            AnsiConsole.WriteLine();
        }

        if (!string.IsNullOrWhiteSpace(result.ModifiedText))
        {
            AnsiConsole.MarkupLine("[bold green]Corrected Text:[/]");
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(result.ModifiedText)}[/]");
        }
    }
}
