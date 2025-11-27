namespace YetAnotherTranslator.Infrastructure.Azure.KeyVault;

internal class KeyVaultOptions
{
    public const string Position = "KeyVault";

    public string VaultName { get; set; } = string.Empty;
}
