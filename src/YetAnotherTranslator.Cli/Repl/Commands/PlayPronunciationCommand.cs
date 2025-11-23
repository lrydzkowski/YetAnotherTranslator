using Spectre.Console;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Models;

namespace YetAnotherTranslator.Cli.Repl.Commands;

internal class PlayPronunciationCommand
{
    private readonly IPlayPronunciationHandler _playPronunciationHandler;

    public PlayPronunciationCommand(IPlayPronunciationHandler playPronunciationHandler)
    {
        _playPronunciationHandler = playPronunciationHandler;
    }

    public async Task HandlePlayPronunciationAsync(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            AnsiConsole.MarkupLine("[yellow]Please provide text for pronunciation.[/]");
            return;
        }

        await AnsiConsole.Status()
            .StartAsync(
                "Playing pronunciation...",
                async ctx =>
                {
                    string text = ParsePartOfSpeech(command, out string? partOfSpeech);
                    PlayPronunciationRequest request = new(command.Type, text, partOfSpeech, !command.NoCache);
                    PronunciationResult result =
                        await _playPronunciationHandler.HandleAsync(request, cancellationToken);
                    ctx.Status("Done");
                    Display(result);
                }
            );
    }

    private static string ParsePartOfSpeech(Command command, out string? partOfSpeech)
    {
        string text = command.Argument;
        partOfSpeech = null;

        if (text.Contains("[pos:") && text.Contains("]"))
        {
            int posStart = text.IndexOf("[pos:", StringComparison.Ordinal);
            int posEnd = text.IndexOf("]", posStart, StringComparison.Ordinal);
            if (posStart >= 0 && posEnd > posStart)
            {
                partOfSpeech = text.Substring(posStart + 5, posEnd - posStart - 5);
                text = text.Substring(0, posStart).Trim();
            }
        }

        return text;
    }

    private void Display(PronunciationResult result)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]✓ Played pronunciation for:[/] {Markup.Escape(result.Text)}");
        if (result.PartOfSpeech is not null)
        {
            AnsiConsole.MarkupLine($"[dim]Part of speech: {Markup.Escape(result.PartOfSpeech)}[/]");
        }
    }
}
