using FluentValidation;
using YetAnotherTranslator.Core.Constants;

namespace YetAnotherTranslator.Core.Handlers.TranslateText;

public class TranslateTextValidator : AbstractValidator<TranslateTextRequest>
{

    public TranslateTextValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text cannot be empty")
            .MaximumLength(ValidationConstants.MaxTextLength)
            .WithMessage($"Text cannot exceed {ValidationConstants.MaxTextLength} characters");

        RuleFor(x => x.TargetLanguage)
            .NotEmpty()
            .WithMessage("Target language must be specified")
            .Must(lang => lang == "Polish" || lang == "English")
            .WithMessage("Target language must be either 'Polish' or 'English'");
    }
}
