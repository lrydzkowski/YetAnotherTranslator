using FluentValidation;
using YetAnotherTranslator.Core.Constants;
using YetAnotherTranslator.Core.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

public class TranslateWordValidator : AbstractValidator<TranslateWordRequest>
{
    public TranslateWordValidator()
    {
        RuleFor(x => x.Word)
            .NotEmpty()
            .WithMessage("Word cannot be empty")
            .MaximumLength(ValidationConstants.MaxWordLength)
            .WithMessage($"Word cannot exceed {ValidationConstants.MaxWordLength} characters");

        RuleFor(x => x.SourceLanguage)
            .IsInEnum()
            .WithMessage("Source language must be a valid language")
            .Must(lang => lang == SourceLanguage.Polish || lang == SourceLanguage.English)
            .WithMessage("Source language must be either Polish or English (auto-detect not supported for word translation)");

        RuleFor(x => x.TargetLanguage)
            .NotEmpty()
            .WithMessage("Target language must be specified")
            .Must(lang => lang == "Polish" || lang == "English")
            .WithMessage("Target language must be either 'Polish' or 'English'");

        RuleFor(x => x)
            .Must(req => req.SourceLanguage.ToString() != req.TargetLanguage)
            .WithMessage("Source and target languages must be different");
    }
}
