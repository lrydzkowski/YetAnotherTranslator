using FluentValidation;

namespace YetAnotherTranslator.Core.Handlers.TranslateText;

public class TranslateTextValidator : AbstractValidator<TranslateTextRequest>
{
    private const int MaxTextLength = 5000;

    public TranslateTextValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text cannot be empty")
            .MaximumLength(MaxTextLength)
            .WithMessage($"Text cannot exceed {MaxTextLength} characters");

        RuleFor(x => x.TargetLanguage)
            .NotEmpty()
            .WithMessage("Target language must be specified")
            .Must(lang => lang == "Polish" || lang == "English")
            .WithMessage("Target language must be either 'Polish' or 'English'");
    }
}
