using FluentValidation;
using YetAnotherTranslator.Core.Common.Extensions;
using YetAnotherTranslator.Core.Handlers.TranslateWord.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateWord;

internal class TranslateWordValidator : AbstractValidator<TranslateWordRequest>
{
    public TranslateWordValidator()
    {
        RuleFor(request => request.Word)
            .NotEmpty()
            .WithMessage("Word cannot be empty")
            .MaximumLength(TranslatorConstants.Validation.MaxWordLength)
            .WithMessage($"Word cannot exceed {TranslatorConstants.Validation.MaxWordLength} characters");

        RuleFor(request => request.SourceLanguage)
            .Must(TranslatorConstants.Languages.IsSupported)
            .When(request => request.SourceLanguage is not null)
            .WithMessage(
                $"Source language must be one of the following: {TranslatorConstants.Languages.Serialize()}"
            );

        RuleFor(request => request.TargetLanguage)
            .Must(TranslatorConstants.Languages.IsSupported)
            .When(request => request.TargetLanguage is not null)
            .WithMessage(
                $"Target language must be one of the following: {TranslatorConstants.Languages.Serialize()}"
            );

        RuleFor(x => x)
            .Must(request => !request.SourceLanguage.EqualsIgnoreCase(request.TargetLanguage))
            .When(request => request.SourceLanguage is not null || request.TargetLanguage is not null)
            .WithMessage("Source and target languages must be different");
    }
}
