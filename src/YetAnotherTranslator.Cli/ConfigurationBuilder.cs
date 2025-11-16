using Microsoft.Extensions.Configuration;
using YetAnotherTranslator.Infrastructure.Azure;

namespace YetAnotherTranslator.Cli;

internal static class ConfigurationBuilder
{
    public static void AddConfiguration(
        this IConfigurationBuilder configurationBuilder,
        string[] args
    )
    {
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
        configurationBuilder.AddJsonFile("appsettings.json", false, true);
        configurationBuilder.AddEnvironmentVariables();
        configurationBuilder.AddAzureKeyVaultConfiguration(configurationBuilder.Build());
        configurationBuilder.AddCommandLine(args);
    }
}
