using PrettyPrompt;
using Spectre.Console;
using YetAnotherTranslator.Cli.Repl.Commands;
using YetAnotherTranslator.Core.Common.Exceptions;
using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Cli.Repl;

internal class ReplEngine
{
    private readonly GetHistoryCommand _getHistoryCommand;
    private readonly CommandParser _parser;
    private readonly PlayPronunciationCommand _playPronunciationCommand;
    private readonly Prompt _prompt = new();
    private readonly ReviewGrammarCommand _reviewGrammarCommand;
    private readonly TranslateTextCommand _translateTextCommand;
    private readonly TranslateWordCommand _translateWordCommand;

    public ReplEngine(
        CommandParser parser,
        TranslateWordCommand translateWordCommand,
        TranslateTextCommand translateTextCommand,
        ReviewGrammarCommand reviewGrammarCommand,
        PlayPronunciationCommand playPronunciationCommand,
        GetHistoryCommand getHistoryCommand
    )
    {
        _parser = parser;
        _translateWordCommand = translateWordCommand;
        _translateTextCommand = translateTextCommand;
        _reviewGrammarCommand = reviewGrammarCommand;
        _playPronunciationCommand = playPronunciationCommand;
        _getHistoryCommand = getHistoryCommand;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine("[bold blue]Yet Another Translator[/]");
        AnsiConsole.MarkupLine("[dim]Type /help for available commands or /quit to exit[/]");
        AnsiConsole.WriteLine();

        while (!cancellationToken.IsCancellationRequested)
        {
            PromptResult promptResult = await _prompt.ReadLineAsync();
            if (!promptResult.IsSuccess)
            {
                continue;
            }

            string input = promptResult.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            try
            {
                Command command = _parser.Parse(input);
                if (await HandleCommandAsync(command, cancellationToken))
                {
                    break;
                }
            }
            catch (ValidationException ex)
            {
                AnsiConsole.MarkupLine($"[red]Validation error: {Markup.Escape(ex.Message)}[/]");
            }
            catch (ExternalServiceException ex)
            {
                AnsiConsole.MarkupLine($"[red]Service error ({ex.ServiceName}): {Markup.Escape(ex.Message)}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
            }

            AnsiConsole.WriteLine();
        }
    }

    private async Task<bool> HandleCommandAsync(Command command, CancellationToken cancellationToken)
    {
        switch (command.Type)
        {
            case CommandType.Quit:
                return true;

            case CommandType.Clear:
                AnsiConsole.Clear();
                return false;

            case CommandType.Help:
                DisplayHelp();
                return false;

            case CommandType.TranslateWord:
                await _translateWordCommand.HandleTranslateWordAsync(command, cancellationToken);
                return false;

            case CommandType.TranslateText:
                await _translateTextCommand.HandleTranslateTextAsync(command, cancellationToken);
                return false;

            case CommandType.ReviewGrammar:
                await _reviewGrammarCommand.HandleReviewGrammarAsync(command, cancellationToken);
                return false;

            case CommandType.PlayPronunciation:
                await _playPronunciationCommand.HandlePlayPronunciationAsync(command, cancellationToken);
                return false;

            case CommandType.GetHistory:
                await _getHistoryCommand.HandleGetHistoryAsync(cancellationToken);
                return false;

            case CommandType.Invalid:
                AnsiConsole.MarkupLine("[yellow]Invalid command. Type /help for available commands.[/]");
                return false;

            default:
                AnsiConsole.MarkupLine("[yellow]Command not yet implemented.[/]");
                return false;
        }
    }

    private static void DisplayHelp()
    {
        Table table = new();
        table.Border(TableBorder.Rounded);
        table.AddColumn(new TableColumn("[bold]Command[/]"));
        table.AddColumn(new TableColumn("[bold]Description[/]"));

        table.AddRow("/t, /translate <word>", "Auto-detect language and translate word");
        table.AddRow("/tp, /translate-polish <word>", "Translate Polish word to English");
        table.AddRow("/te, /translate-english <word>", "Translate English word to Polish");
        table.AddRow("/tt, /translate-text <text>", "Auto-detect and translate text");
        table.AddRow("/ttp, /translate-text-polish <text>", "Translate Polish text to English");
        table.AddRow("/tte, /translate-text-english <text>", "Translate English text to Polish");
        table.AddRow("/r, /review <text>", "Review English grammar and vocabulary");
        table.AddRow("/p, /playback <word>", "Play pronunciation of English word");
        table.AddRow("/hist, /history", "Show operation history");
        table.AddRow("/help", "Show this help message");
        table.AddRow("/clear, /c", "Clear the screen");
        table.AddRow("/quit, /q", "Exit the application");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Use --no-cache flag to bypass cache (e.g., /t cat --no-cache)[/]");
    }
}
