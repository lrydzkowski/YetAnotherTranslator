using Spectre.Console;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Handlers.TranslateText.Models;

namespace YetAnotherTranslator.Cli.Repl.Commands;

internal class TranslateTextCommand
{
    private readonly TranslateTextHandler _translateTextHandler;

    public TranslateTextCommand(TranslateTextHandler translateTextHandler)
    {
        _translateTextHandler = translateTextHandler;
    }

    public async Task HandleTranslateTextAsync(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            AnsiConsole.MarkupLine("[yellow]Please provide text to translate.[/]");

            return;
        }

        await AnsiConsole.Status()
            .StartAsync(
                "Translating...",
                async ctx =>
                {
                    TranslateTextRequest request = new(
                        command.Argument,
                        command.SourceLanguage,
                        command.TargetLanguage,
                        !command.NoCache
                    );
                    TextTranslationResult result = await _translateTextHandler.HandleAsync(request, cancellationToken);
                    ctx.Status("Done");
                    Display(result);
                }
            );
    }

    private static void Display(TextTranslationResult result)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]{result.SourceLanguage} → {result.TargetLanguage}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Original:[/]");
        AnsiConsole.WriteLine(result.InputText);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold green]Translation:[/]");
        AnsiConsole.WriteLine(result.TranslatedText);
    }
}
