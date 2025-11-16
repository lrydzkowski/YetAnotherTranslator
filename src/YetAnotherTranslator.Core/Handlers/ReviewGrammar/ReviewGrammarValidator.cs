using FluentValidation;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

internal class ReviewGrammarValidator : AbstractValidator<ReviewGrammarRequest>
{
    public ReviewGrammarValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text cannot be empty")
            .MaximumLength(TranslatorConstants.Validation.MaxTextLength)
            .WithMessage($"Text cannot exceed {TranslatorConstants.Validation.MaxTextLength} characters");
    }
}
