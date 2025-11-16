using FluentValidation;
using YetAnotherTranslator.Core.Handlers.GetHistory.Models;

namespace YetAnotherTranslator.Core.Handlers.GetHistory;

internal class GetHistoryValidator : AbstractValidator<GetHistoryRequest>
{
    public GetHistoryValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(
                TranslatorConstants.Validation.MinHistoryLimit,
                TranslatorConstants.Validation.MaxHistoryLimit
            )
            .WithMessage(
                $"Limit must be between {TranslatorConstants.Validation.MinHistoryLimit} and {TranslatorConstants.Validation.MaxHistoryLimit}"
            );
    }
}
