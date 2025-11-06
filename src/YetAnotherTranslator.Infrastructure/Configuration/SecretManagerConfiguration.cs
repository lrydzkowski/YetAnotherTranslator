namespace YetAnotherTranslator.Infrastructure.Configuration;

public class SecretManagerConfiguration
{
    public string Provider { get; set; } = string.Empty;
    public string KeyVaultUrl { get; set; } = string.Empty;
}
