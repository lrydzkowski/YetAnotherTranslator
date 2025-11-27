using FluentValidation;
using YetAnotherTranslator.Core.Common.Extensions;
using YetAnotherTranslator.Core.Handlers.TranslateText.Models;

namespace YetAnotherTranslator.Core.Handlers.TranslateText;

internal class TranslateTextValidator : AbstractValidator<TranslateTextRequest>
{
    public TranslateTextValidator()
    {
        RuleFor(request => request.Text)
            .NotEmpty()
            .WithMessage("Text cannot be empty")
            .MaximumLength(TranslatorConstants.Validation.MaxTextLength)
            .WithMessage($"Text cannot exceed {TranslatorConstants.Validation.MaxTextLength} characters");

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
