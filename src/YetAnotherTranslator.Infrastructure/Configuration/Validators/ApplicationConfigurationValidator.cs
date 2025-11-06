using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.Configuration.Validators;

public class ApplicationConfigurationValidator : AbstractValidator<ApplicationConfiguration>
{
    public ApplicationConfigurationValidator()
    {
        RuleFor(x => x.SecretManager)
            .NotNull()
            .WithMessage("SecretManager configuration is required")
            .SetValidator(new SecretManagerConfigurationValidator());

        RuleFor(x => x.LlmProvider)
            .NotNull()
            .WithMessage("LlmProvider configuration is required")
            .SetValidator(new LlmProviderConfigurationValidator());

        RuleFor(x => x.TtsProvider)
            .NotNull()
            .WithMessage("TtsProvider configuration is required")
            .SetValidator(new TtsProviderConfigurationValidator());

        RuleFor(x => x.Database)
            .NotNull()
            .WithMessage("Database configuration is required")
            .SetValidator(new DatabaseConfigurationValidator());
    }
}
