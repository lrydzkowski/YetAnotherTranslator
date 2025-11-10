using FluentValidation;

namespace YetAnotherTranslator.Core.Handlers.PlayPronunciation;

public class PlayPronunciationValidator : AbstractValidator<PlayPronunciationRequest>
{
    private const int MaxTextLength = 500;

    public PlayPronunciationValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text cannot be empty")
            .MaximumLength(MaxTextLength)
            .WithMessage($"Text cannot exceed {MaxTextLength} characters for pronunciation");
    }
}
