namespace YetAnotherTranslator.Infrastructure.Azure.AiFoundry;

internal class AzureAiFoundryOptions
{
    public const string Position = "AzureAiFoundry";

    public string Endpoint { get; init; } = "";
    public string DeploymentName { get; init; } = "";
    public string ApiKey { get; init; } = "";
}
