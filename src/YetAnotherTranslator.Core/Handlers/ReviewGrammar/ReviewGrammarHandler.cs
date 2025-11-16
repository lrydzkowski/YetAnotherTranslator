using FluentValidation;
using YetAnotherTranslator.Core.Common.Models;
using YetAnotherTranslator.Core.Common.Services;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Interfaces;
using YetAnotherTranslator.Core.Handlers.ReviewGrammar.Models;

namespace YetAnotherTranslator.Core.Handlers.ReviewGrammar;

public class ReviewGrammarHandler
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ILargeLanguageModelProvider _largeLanguageModelProvider;
    private readonly ISerializer _serializer;
    private readonly IValidator<ReviewGrammarRequest> _validator;

    public ReviewGrammarHandler(
        ILargeLanguageModelProvider largeLanguageModelProvider,
        IValidator<ReviewGrammarRequest> validator,
        IHistoryRepository historyRepository,
        ISerializer serializer
    )
    {
        _largeLanguageModelProvider = largeLanguageModelProvider;
        _validator = validator;
        _historyRepository = historyRepository;
        _serializer = serializer;
    }

    public async Task<GrammarReviewResult?> HandleAsync(
        ReviewGrammarRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        GrammarReviewResult? result = await _largeLanguageModelProvider.ReviewGrammarAsync(
            request.Text,
            cancellationToken
        );

        await _historyRepository.SaveHistoryAsync(
            CommandType.ReviewGrammar,
            request.Text,
            result is null ? null : _serializer.Serialize(result),
            cancellationToken
        );

        return result;
    }
}
