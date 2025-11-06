using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.Configuration.Validators;

public class TtsProviderConfigurationValidator : AbstractValidator<TtsProviderConfiguration>
{
    public TtsProviderConfigurationValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("TtsProvider.Provider is required")
            .Must(provider => provider == "ElevenLabs")
            .WithMessage("TtsProvider.Provider must be 'ElevenLabs'");

        RuleFor(x => x.ApiKeySecretName)
            .NotEmpty()
            .WithMessage("TtsProvider.ApiKeySecretName is required");

        RuleFor(x => x.VoiceId)
            .NotEmpty()
            .WithMessage("TtsProvider.VoiceId is required");
    }
}
