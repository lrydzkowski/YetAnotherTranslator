using FluentValidation;

namespace YetAnotherTranslator.Core.Handlers.GetHistory;

public class GetHistoryValidator : AbstractValidator<GetHistoryRequest>
{
    private const int MinLimit = 1;
    private const int MaxLimit = 1000;

    public GetHistoryValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(MinLimit, MaxLimit)
            .WithMessage($"Limit must be between {MinLimit} and {MaxLimit}");
    }
}
