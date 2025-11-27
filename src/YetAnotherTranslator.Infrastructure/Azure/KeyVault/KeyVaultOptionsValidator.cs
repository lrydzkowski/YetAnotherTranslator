using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.Azure.KeyVault;

internal class KeyVaultOptionsValidator : AbstractValidator<KeyVaultOptions>
{
    public KeyVaultOptionsValidator()
    {
        DefineVaultNameValidator();
    }

    private void DefineVaultNameValidator()
    {
        RuleFor(x => x.VaultName)
            .NotEmpty()
            .WithName(nameof(KeyVaultOptions.VaultName))
            .OverridePropertyName($"{KeyVaultOptions.Position}.{nameof(KeyVaultOptions.VaultName)}");
    }
}
