using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.OpenAI;

internal class OpenAiOptionsValidator : AbstractValidator<OpenAiOptions>
{
    public OpenAiOptionsValidator()
    {
        DefineApiKeyValidator();
        DefineModelNameValidator();
    }

    private void DefineApiKeyValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .WithName(nameof(OpenAiOptions.ApiKey))
            .OverridePropertyName($"{OpenAiOptions.Position}.{nameof(OpenAiOptions.ApiKey)}");
    }

    private void DefineModelNameValidator()
    {
        RuleFor(x => x.ModelName)
            .NotEmpty()
            .WithName(nameof(OpenAiOptions.ModelName))
            .OverridePropertyName($"{OpenAiOptions.Position}.{nameof(OpenAiOptions.ModelName)}");
    }
}
