using Microsoft.Extensions.Configuration;
using YetAnotherTranslator.Core;
using YetAnotherTranslator.Core.Common.Extensions;
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

        if (IsDevelopment())
        {
            configurationBuilder.AddUserSecrets(typeof(ConfigurationBuilder).Assembly);
        }
    }

    private static bool IsDevelopment()
    {
        string environment = Environment.GetEnvironmentVariable(TranslatorConstants.EnvironmentVariables.Environment)
            ?? "";

        return environment.EqualsIgnoreCase(TranslatorConstants.Environments.Development);
    }
}
