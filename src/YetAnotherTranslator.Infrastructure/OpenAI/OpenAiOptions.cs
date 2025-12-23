namespace YetAnotherTranslator.Infrastructure.OpenAI;

internal class OpenAiOptions
{
    public const string Position = "OpenAI";

    public string ApiKey { get; init; } = "";
    public string ModelName { get; init; } = "gpt-5-nano";
}
