using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.Azure.AiFoundry;

internal class AzureAiFoundryOptionsValidator : AbstractValidator<AzureAiFoundryOptions>
{
    public AzureAiFoundryOptionsValidator()
    {
        DefineEndpointValidator();
        DefineDeploymentNameValidator();
        DefineApiKeyValidator();
    }

    private void DefineEndpointValidator()
    {
        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithName(nameof(AzureAiFoundryOptions.Endpoint))
            .OverridePropertyName($"{AzureAiFoundryOptions.Position}.{nameof(AzureAiFoundryOptions.Endpoint)}");
    }

    private void DefineDeploymentNameValidator()
    {
        RuleFor(x => x.DeploymentName)
            .NotEmpty()
            .WithName(nameof(AzureAiFoundryOptions.DeploymentName))
            .OverridePropertyName($"{AzureAiFoundryOptions.Position}.{nameof(AzureAiFoundryOptions.DeploymentName)}");
    }

    private void DefineApiKeyValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .WithName(nameof(AzureAiFoundryOptions.ApiKey))
            .OverridePropertyName($"{AzureAiFoundryOptions.Position}.{nameof(AzureAiFoundryOptions.ApiKey)}");
    }
}
