using FluentValidation;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

public class ReviewGrammarValidator : AbstractValidator<ReviewGrammarRequest>
{
    private const int MaxTextLength = 5000;

    public ReviewGrammarValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text cannot be empty")
            .MaximumLength(MaxTextLength)
            .WithMessage($"Text cannot exceed {MaxTextLength} characters");
    }
}
