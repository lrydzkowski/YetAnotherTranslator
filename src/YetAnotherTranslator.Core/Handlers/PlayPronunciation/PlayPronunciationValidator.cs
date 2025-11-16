using FluentValidation;
using YetAnotherTranslator.Core.Handlers.PlayPronunciation.Models;

namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation;

internal class PlayPronunciationValidator : AbstractValidator<PlayPronunciationRequest>
{
    public PlayPronunciationValidator()
    {
        RuleFor(request => request.Text)
            .NotEmpty()
            .WithMessage("Text cannot be empty")
            .MaximumLength(TranslatorConstants.Validation.MaxTextLength)
            .WithMessage(
                $"Text cannot exceed {TranslatorConstants.Validation.MaxTextLength} characters for pronunciation"
            );

        RuleFor(request => request.PartOfSpeech)
            .Must(TranslatorConstants.PartsOfSpeech.IsSupported)
            .WithMessage(
                $"Part of speech must be one of the following: {TranslatorConstants.PartsOfSpeech.Serialize()}"
            );
    }
}
