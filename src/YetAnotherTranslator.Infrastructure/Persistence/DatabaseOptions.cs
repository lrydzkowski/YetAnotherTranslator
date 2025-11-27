using Microsoft.Extensions.Configuration;

namespace YetAnotherTranslator.Infrastructure.Persistence;

internal class DatabaseOptions
{
    public const string Position = "Database";

    public string ConnectionString { get; set; } = string.Empty;

    public static string GetConnectionString(IConfiguration configuration)
    {
        return configuration[$"{Position}:{nameof(ConnectionString)}"] ?? string.Empty;
    }
}
