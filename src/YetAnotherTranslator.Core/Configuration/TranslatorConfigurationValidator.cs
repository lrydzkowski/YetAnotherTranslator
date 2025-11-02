using FluentValidation;

namespace YetAnotherTranslator.Core.Configuration;

public class TranslatorConfigurationValidator : AbstractValidator<TranslatorConfiguration>
{
    public TranslatorConfigurationValidator()
    {
        RuleFor(x => x.Azure.KeyVaultUrl)
            .NotEmpty()
            .WithMessage("Azure Key Vault URL must be configured")
            .Must(BeValidUrl)
            .WithMessage("Azure Key Vault URL must be a valid HTTPS URL");

        RuleFor(x => x.Database.ConnectionString)
            .NotEmpty()
            .WithMessage("Database connection string must be configured")
            .Must(BeValidPostgreSqlConnectionString)
            .WithMessage("Database connection string must be a valid PostgreSQL connection string");

        RuleFor(x => x.ElevenLabs.DefaultVoiceId)
            .NotEmpty()
            .WithMessage("ElevenLabs default voice ID must be configured");
    }

    private bool BeValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
    }

    private bool BeValidPostgreSqlConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        return connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) &&
               connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase);
    }
}
