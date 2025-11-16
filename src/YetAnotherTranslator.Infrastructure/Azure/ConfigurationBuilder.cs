using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using YetAnotherTranslator.Infrastructure.Azure.KeyVault;

namespace YetAnotherTranslator.Infrastructure.Azure;

public static class ConfigurationBuilder
{
    public static void AddAzureKeyVaultConfiguration(
        this IConfigurationBuilder configurationBuilder,
        IConfiguration configuration
    )
    {
        string? keyVaultName = configuration[$"{KeyVaultOptions.Position}:{nameof(KeyVaultOptions.VaultName)}"];
        if (string.IsNullOrWhiteSpace(keyVaultName))
        {
            return;
        }

        SecretClient secretClient = new(
            new Uri($"https://{keyVaultName}.vault.azure.net/"),
            new DefaultAzureCredential()
        );

        configurationBuilder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
    }
}
