using FluentValidation;
using YetAnotherTranslator.Core.Constants;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

public class ReviewGrammarValidator : AbstractValidator<ReviewGrammarRequest>
{

    public ReviewGrammarValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text cannot be empty")
            .MaximumLength(ValidationConstants.MaxTextLength)
            .WithMessage($"Text cannot exceed {ValidationConstants.MaxTextLength} characters");
    }
}
