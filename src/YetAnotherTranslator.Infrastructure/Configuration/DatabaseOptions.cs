namespace YetAnotherTranslator.Infrastructure.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionStringSecretName { get; set; } = string.Empty;
}
