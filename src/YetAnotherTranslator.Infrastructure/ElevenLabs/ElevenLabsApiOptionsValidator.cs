using FluentValidation;

namespace YetAnotherTranslator.Infrastructure.ElevenLabs;

internal class ElevenLabsApiOptionsValidator : AbstractValidator<ElevenLabsApiOptions>
{
    public ElevenLabsApiOptionsValidator()
    {
        DefineApiKeyValidator();
        DefineVoiceIdValidator();
    }

    private void DefineApiKeyValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .WithName(nameof(ElevenLabsApiOptions.ApiKey))
            .OverridePropertyName($"{ElevenLabsApiOptions.Position}.{nameof(ElevenLabsApiOptions.ApiKey)}");
    }

    private void DefineVoiceIdValidator()
    {
        RuleFor(x => x.VoiceId)
            .NotEmpty()
            .WithName(nameof(ElevenLabsApiOptions.VoiceId))
            .OverridePropertyName($"{ElevenLabsApiOptions.Position}.{nameof(ElevenLabsApiOptions.VoiceId)}");
    }
}
