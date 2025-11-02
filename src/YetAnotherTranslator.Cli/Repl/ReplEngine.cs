using PrettyPrompt;
using PrettyPrompt.Highlighting;
using Spectre.Console;
using YetAnotherTranslator.Cli.Output;
using YetAnotherTranslator.Core.Handlers;
using YetAnotherTranslator.Core.Models;
using YetAnotherTranslator.Core.Repl;

namespace YetAnotherTranslator.Cli.Repl;

public class ReplEngine
{
    private readonly TranslateWordHandler _translateWordHandler;

    public ReplEngine(TranslateWordHandler translateWordHandler)
    {
        _translateWordHandler = translateWordHandler;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        TranslationFormatter.DisplayWelcome();

        var prompt = new Prompt(
            callbacks: new ReplCallbacks(),
            configuration: new PromptConfiguration(
                prompt: new FormattedString("> ", new FormatSpan(0, 2, AnsiColor.Green))));

        while (!cancellationToken.IsCancellationRequested)
        {
            var response = await prompt.ReadLineAsync();

            if (!response.IsSuccess)
            {
                break;
            }

            var input = response.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            var parsedCommand = CommandParser.Parse(input);

            try
            {
                switch (parsedCommand.Type)
                {
                    case CommandType.Quit:
                        return;

                    case CommandType.Help:
                        TranslationFormatter.DisplayHelp();
                        break;

                    case CommandType.Clear:
                        AnsiConsole.Clear();
                        TranslationFormatter.DisplayWelcome();
                        break;

                    case CommandType.ViewHistory:
                        TranslationFormatter.DisplayError("History feature not yet implemented");
                        break;

                    case CommandType.TranslateWord:
                        if (parsedCommand.Data is Core.Commands.TranslateWordCommand command)
                        {
                            await HandleTranslateWordAsync(command, cancellationToken);
                        }
                        break;

                    case CommandType.TranslateText:
                        TranslationFormatter.DisplayError("Text translation not yet implemented");
                        break;

                    case CommandType.Invalid:
                        TranslationFormatter.DisplayError("Invalid command. Type /help for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                TranslationFormatter.DisplayError($"An error occurred: {ex.Message}");
            }
        }
    }

    private async Task HandleTranslateWordAsync(
        Core.Commands.TranslateWordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[grey]Translating...[/]", async ctx =>
            {
                return await _translateWordHandler.HandleAsync(command, cancellationToken);
            });

        TranslationFormatter.DisplayTranslation(result);
    }
}

internal class ReplCallbacks : PromptCallbacks
{
    protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var spans = new List<FormatSpan>();

        if (text.StartsWith('/'))
        {
            spans.Add(new FormatSpan(0, text.Length, AnsiColor.Cyan));
        }

        return Task.FromResult<IReadOnlyCollection<FormatSpan>>(spans);
    }
}
