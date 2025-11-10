namespace YetAnotherTranslator.Infrastructure.Configuration;

public class KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    public string VaultName { get; set; } = string.Empty;

    public string VaultUri => string.IsNullOrWhiteSpace(VaultName)
        ? string.Empty
        : $"https://{VaultName}.vault.azure.net";
}
