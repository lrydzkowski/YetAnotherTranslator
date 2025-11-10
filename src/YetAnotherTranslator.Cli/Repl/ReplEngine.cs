using PrettyPrompt;
using Spectre.Console;
using YetAnotherTranslator.Cli.Display;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Core.Handlers.GetHistory;
using YetAnotherTranslator.Core.Handlers.TranslateWord;
using YetAnotherTranslator.Core.Handlers.TranslateText;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation;
using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Cli.Repl;

public class ReplEngine
{
    private readonly CommandParser _parser;
    private readonly TranslateWordHandler _translateWordHandler;
    private readonly TranslateTextHandler _translateTextHandler;
    private readonly ReviewGrammarHandler _reviewGrammarHandler;
    private readonly PlayPronunciationHandler _playPronunciationHandler;
    private readonly Prompt _prompt;

    public ReplEngine(
        CommandParser parser,
        TranslateWordHandler translateWordHandler,
        TranslateTextHandler translateTextHandler,
        ReviewGrammarHandler reviewGrammarHandler,
        PlayPronunciationHandler playPronunciationHandler)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _translateWordHandler = translateWordHandler ?? throw new ArgumentNullException(nameof(translateWordHandler));
        _translateTextHandler = translateTextHandler ?? throw new ArgumentNullException(nameof(translateTextHandler));
        _reviewGrammarHandler = reviewGrammarHandler ?? throw new ArgumentNullException(nameof(reviewGrammarHandler));
        _playPronunciationHandler = playPronunciationHandler ?? throw new ArgumentNullException(nameof(playPronunciationHandler));
        _prompt = new Prompt();
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
                await HandleTranslateWordAsync(command, cancellationToken);
                return false;

            case CommandType.TranslateText:
                await HandleTranslateTextAsync(command, cancellationToken);
                return false;

            case CommandType.ReviewGrammar:
                await HandleReviewGrammarAsync(command, cancellationToken);
                return false;

            case CommandType.PlayPronunciation:
                await HandlePlayPronunciationAsync(command, cancellationToken);
                return false;

            case CommandType.Invalid:
                AnsiConsole.MarkupLine("[yellow]Invalid command. Type /help for available commands.[/]");
                return false;

            default:
                AnsiConsole.MarkupLine("[yellow]Command not yet implemented.[/]");
                return false;
        }
    }

    private async Task HandleTranslateWordAsync(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            AnsiConsole.MarkupLine("[yellow]Please provide a word to translate.[/]");
            return;
        }

        string sourceLanguage;
        string targetLanguage;

        if (command.AutoDetectLanguage)
        {
            AnsiConsole.MarkupLine("[dim]Auto-detection not yet implemented. Using Polish→English.[/]");
            sourceLanguage = "Polish";
            targetLanguage = "English";
        }
        else
        {
            sourceLanguage = command.SourceLanguage ?? "Polish";
            targetLanguage = command.TargetLanguage ?? "English";
        }

        var request = new TranslateWordRequest(
            command.Argument,
            sourceLanguage,
            targetLanguage,
            !command.NoCache
        );

        await AnsiConsole.Status()
            .StartAsync("Translating...", async ctx =>
            {
                TranslationResult result = await _translateWordHandler.HandleAsync(request, cancellationToken);
                ctx.Status("Done");
                AnsiConsole.WriteLine();
                TranslationTableFormatter.DisplayTranslations(result);
            });
    }

    private async Task HandleTranslateTextAsync(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            AnsiConsole.MarkupLine("[yellow]Please provide text to translate.[/]");
            return;
        }

        SourceLanguage sourceLanguage;
        string targetLanguage;

        if (command.AutoDetectLanguage)
        {
            sourceLanguage = SourceLanguage.Auto;
            targetLanguage = "English";
        }
        else if (command.SourceLanguage == "Polish")
        {
            sourceLanguage = SourceLanguage.Polish;
            targetLanguage = command.TargetLanguage ?? "English";
        }
        else if (command.SourceLanguage == "English")
        {
            sourceLanguage = SourceLanguage.English;
            targetLanguage = command.TargetLanguage ?? "Polish";
        }
        else
        {
            sourceLanguage = SourceLanguage.Auto;
            targetLanguage = command.TargetLanguage ?? "English";
        }

        var request = new TranslateTextRequest(
            command.Argument,
            sourceLanguage,
            targetLanguage,
            !command.NoCache
        );

        await AnsiConsole.Status()
            .StartAsync("Translating...", async ctx =>
            {
                TextTranslationResult result = await _translateTextHandler.HandleAsync(request, cancellationToken);
                ctx.Status("Done");
                AnsiConsole.WriteLine();
                TextTranslationFormatter.Display(result);
            });
    }

    private async Task HandleReviewGrammarAsync(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            AnsiConsole.MarkupLine("[yellow]Please provide text to review.[/]");
            return;
        }

        var request = new ReviewGrammarRequest(command.Argument);

        await AnsiConsole.Status()
            .StartAsync("Reviewing grammar...", async ctx =>
            {
                GrammarReviewResult result = await _reviewGrammarHandler.HandleAsync(request, cancellationToken);
                ctx.Status("Done");
                AnsiConsole.WriteLine();
                GrammarReviewFormatter.Display(result);
            });
    }

    private async Task HandlePlayPronunciationAsync(Command command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Argument))
        {
            AnsiConsole.MarkupLine("[yellow]Please provide text for pronunciation.[/]");
            return;
        }

        // Parse the argument to check for part-of-speech parameter
        // Format: "word [pos:noun]"
        string text = command.Argument;
        string? partOfSpeech = null;

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

        var request = new PlayPronunciationRequest(text, partOfSpeech, !command.NoCache);

        await AnsiConsole.Status()
            .StartAsync("Playing pronunciation...", async ctx =>
            {
                PronunciationResult result = await _playPronunciationHandler.HandleAsync(request, cancellationToken);
                ctx.Status("Done");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[green]✓ Played pronunciation for:[/] {Markup.Escape(result.Text)}");
                if (result.PartOfSpeech != null)
                {
                    AnsiConsole.MarkupLine($"[dim]Part of speech: {Markup.Escape(result.PartOfSpeech)}[/]");
                }
            });
    }

    private static void DisplayHelp()
    {
        var table = new Table();
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
