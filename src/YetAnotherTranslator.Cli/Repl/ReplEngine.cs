using Microsoft.Extensions.Logging;
using PrettyPrompt;
using Spectre.Console;
using YetAnotherTranslator.Cli.Repl.Commands;
using YetAnotherTranslator.Core.Common.Exceptions;
using YetAnotherTranslator.Core.Common.Models;

namespace YetAnotherTranslator.Cli.Repl;

internal class ReplEngine
{
    private readonly GetHistoryCommand _getHistoryCommand;
    private readonly ILogger<ReplEngine> _logger;
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
        GetHistoryCommand getHistoryCommand,
        ILogger<ReplEngine> logger
    )
    {
        _parser = parser;
        _translateWordCommand = translateWordCommand;
        _translateTextCommand = translateTextCommand;
        _reviewGrammarCommand = reviewGrammarCommand;
        _playPronunciationCommand = playPronunciationCommand;
        _getHistoryCommand = getHistoryCommand;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        DisplayTitle();

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
                _logger.LogError(ex, "Validation error");
                AnsiConsole.MarkupLine($"[red]Validation error: {Markup.Escape(ex.Message)}[/]");
                AnsiConsole.WriteLine();
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "External service error: {ServiceName}", ex.ServiceName);
                AnsiConsole.MarkupLine($"[red]Service error ({ex.ServiceName}): {Markup.Escape(ex.Message)}[/]");
                AnsiConsole.WriteLine();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in REPL");
                AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
                AnsiConsole.WriteLine();
            }
        }
    }

    private async Task<bool> HandleCommandAsync(Command command, CancellationToken cancellationToken)
    {
        bool shouldQuit = command.Type switch
        {
            CommandType.Quit => true,
            CommandType.Clear => HandleClearCommand(),
            CommandType.Help => HandleHelpCommand(),
            CommandType.TranslateWord => await HandleTranslateWordCommandAsync(command, cancellationToken),
            CommandType.TranslateText => await HandleTranslateTextCommandAsync(command, cancellationToken),
            CommandType.ReviewGrammar => await HandleReviewGrammarCommandAsync(command, cancellationToken),
            CommandType.PlayPronunciation => await HandlePlayPronunciationCommandAsync(command, cancellationToken),
            CommandType.GetHistory => await HandleGetHistoryCommandAsync(cancellationToken),
            CommandType.Invalid => HandleInvalidCommand(),
            _ => HandleUnimplementedCommand()
        };

        return shouldQuit;
    }

    private bool HandleClearCommand()
    {
        AnsiConsole.Clear();
        DisplayTitle();

        return false;
    }

    private bool HandleHelpCommand()
    {
        DisplayHelp();

        return false;
    }

    private async Task<bool> HandleTranslateWordCommandAsync(Command command, CancellationToken cancellationToken)
    {
        await _translateWordCommand.HandleTranslateWordAsync(command, cancellationToken);

        return false;
    }

    private async Task<bool> HandleTranslateTextCommandAsync(Command command, CancellationToken cancellationToken)
    {
        await _translateTextCommand.HandleTranslateTextAsync(command, cancellationToken);

        return false;
    }

    private async Task<bool> HandleReviewGrammarCommandAsync(Command command, CancellationToken cancellationToken)
    {
        await _reviewGrammarCommand.HandleReviewGrammarAsync(command, cancellationToken);

        return false;
    }

    private async Task<bool> HandlePlayPronunciationCommandAsync(Command command, CancellationToken cancellationToken)
    {
        await _playPronunciationCommand.HandlePlayPronunciationAsync(command, cancellationToken);

        return false;
    }

    private async Task<bool> HandleGetHistoryCommandAsync(CancellationToken cancellationToken)
    {
        await _getHistoryCommand.HandleGetHistoryAsync(cancellationToken);

        return false;
    }

    private bool HandleInvalidCommand()
    {
        AnsiConsole.MarkupLine("[yellow]Invalid command. Type /help for available commands.[/]");
        AnsiConsole.WriteLine();

        return false;
    }

    private bool HandleUnimplementedCommand()
    {
        AnsiConsole.MarkupLine("[yellow]Command not yet implemented.[/]");
        AnsiConsole.WriteLine();

        return false;
    }

    private static void DisplayTitle()
    {
        AnsiConsole.MarkupLine("[bold blue]Yet Another Translator[/]");
        AnsiConsole.MarkupLine("[dim]Type /help for available commands or /quit to exit[/]");
        AnsiConsole.WriteLine();
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
        AnsiConsole.WriteLine();
    }
}
