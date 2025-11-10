using Spectre.Console;
using YetAnotherTranslator.Core.Handlers.TranslateText;

namespace YetAnotherTranslator.Cli.Display;

public static class TextTranslationFormatter
{
    public static void Display(TextTranslationResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        AnsiConsole.MarkupLine($"[bold cyan]{result.SourceLanguage} → {result.TargetLanguage}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Original:[/]");
        AnsiConsole.WriteLine(result.InputText);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold green]Translation:[/]");
        AnsiConsole.WriteLine(result.TranslatedText);
    }
}
