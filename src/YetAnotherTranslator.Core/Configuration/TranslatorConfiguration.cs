namespace YetAnotherTranslator.Core.Configuration;

public class TranslatorConfiguration
{
    public AzureConfiguration Azure { get; set; } = new();
    public DatabaseConfiguration Database { get; set; } = new();
    public ElevenLabsConfiguration ElevenLabs { get; set; } = new();
}

public class AzureConfiguration
{
    public string KeyVaultUrl { get; set; } = string.Empty;
}

public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class ElevenLabsConfiguration
{
    public string DefaultVoiceId { get; set; } = string.Empty;
}
