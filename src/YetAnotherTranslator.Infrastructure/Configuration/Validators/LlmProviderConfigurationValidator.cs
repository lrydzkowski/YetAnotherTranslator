using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.Configuration.Validators;

public class LlmProviderConfigurationValidator : AbstractValidator<LlmProviderConfiguration>
{
    public LlmProviderConfigurationValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("LlmProvider.Provider is required")
            .Must(provider => provider == "Anthropic")
            .WithMessage("LlmProvider.Provider must be 'Anthropic'");

        RuleFor(x => x.Model)
            .NotEmpty()
            .WithMessage("LlmProvider.Model is required");

        RuleFor(x => x.ApiKeySecretName)
            .NotEmpty()
            .WithMessage("LlmProvider.ApiKeySecretName is required");

        RuleFor(x => x.MaxTokens)
            .GreaterThan(0)
            .WithMessage("LlmProvider.MaxTokens must be greater than 0")
            .LessThanOrEqualTo(8192)
            .WithMessage("LlmProvider.MaxTokens must be less than or equal to 8192");

        RuleFor(x => x.Temperature)
            .GreaterThanOrEqualTo(0)
            .WithMessage("LlmProvider.Temperature must be between 0 and 1")
            .LessThanOrEqualTo(1)
            .WithMessage("LlmProvider.Temperature must be between 0 and 1");
    }
}
