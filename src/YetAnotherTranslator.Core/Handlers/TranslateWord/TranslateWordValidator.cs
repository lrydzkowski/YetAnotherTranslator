using FluentValidation;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public class TranslateWordValidator : AbstractValidator<TranslateWordRequest>
{
    private const int MaxWordLength = 100;

    public TranslateWordValidator()
    {
        RuleFor(x => x.Word)
            .NotEmpty()
            .WithMessage("Word cannot be empty")
            .MaximumLength(MaxWordLength)
            .WithMessage($"Word cannot exceed {MaxWordLength} characters");

        RuleFor(x => x.SourceLanguage)
            .NotEmpty()
            .WithMessage("Source language must be specified")
            .Must(lang => lang == "Polish" || lang == "English")
            .WithMessage("Source language must be either 'Polish' or 'English'");

        RuleFor(x => x.TargetLanguage)
            .NotEmpty()
            .WithMessage("Target language must be specified")
            .Must(lang => lang == "Polish" || lang == "English")
            .WithMessage("Target language must be either 'Polish' or 'English'");

        RuleFor(x => x)
            .Must(req => req.SourceLanguage != req.TargetLanguage)
            .WithMessage("Source and target languages must be different");
    }
}
