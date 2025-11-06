using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.Configuration.Validators;

public class SecretManagerConfigurationValidator : AbstractValidator<SecretManagerConfiguration>
{
    public SecretManagerConfigurationValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("SecretManager.Provider is required")
            .Must(provider => provider == "AzureKeyVault")
            .WithMessage("SecretManager.Provider must be 'AzureKeyVault'");

        RuleFor(x => x.KeyVaultUrl)
            .NotEmpty()
            .WithMessage("SecretManager.KeyVaultUrl is required")
            .Must(BeValidUri)
            .WithMessage("SecretManager.KeyVaultUrl must be a valid HTTPS URL");
    }

    private static bool BeValidUri(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttps;
    }
}
